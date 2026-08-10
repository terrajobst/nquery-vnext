using System.Collections.Immutable;

using NQuery.Authoring.CodeActions;
using NQuery.Authoring.LanguageServer.Mapping;
using NQuery.Authoring.LanguageServer.Protocol;
using NQuery.CodeAnalysis.Text;

using StreamJsonRpc;

using LspDiagnostic = NQuery.Authoring.LanguageServer.Protocol.Diagnostic;

namespace NQuery.Authoring.LanguageServer.Server;

internal sealed partial class LanguageServerTarget
{
    private readonly Dictionary<Uri, CancellationTokenSource> _pendingDiagnostics = new();
    private readonly object _diagnosticsGate = new();

    [JsonRpcMethod(Methods.TextDocumentDidOpen, UseSingleObjectParameterDeserialization = true)]
    public void DidOpen(DidOpenTextDocumentParams parameters)
    {
        ThrowIfNull(parameters);

        var document = parameters.TextDocument;
        _documents.Open(document.Uri, document.LanguageId, document.Version, document.Text);

        // No debounce on open: the file is already on screen and the user is waiting.
        _ = PublishDiagnosticsAsync(document.Uri, CancellationToken.None);
    }

    [JsonRpcMethod(Methods.TextDocumentDidChange, UseSingleObjectParameterDeserialization = true)]
    public void DidChange(DidChangeTextDocumentParams parameters)
    {
        ThrowIfNull(parameters);

        var document = parameters.TextDocument;
        if (_documents.Change(document.Uri, document.Version, parameters.ContentChanges))
            ScheduleDiagnostics(document.Uri);
    }

    [JsonRpcMethod(Methods.TextDocumentDidClose, UseSingleObjectParameterDeserialization = true)]
    public void DidClose(DidCloseTextDocumentParams parameters)
    {
        ThrowIfNull(parameters);

        var uri = parameters.TextDocument.Uri;
        CancelPendingDiagnostics(uri);

        if (_documents.Close(uri))
        {
            // Clearing is explicit: a client keeps showing the last published set otherwise.
            _ = SendDiagnosticsAsync(uri, null, []);
        }
    }

    private void ScheduleDiagnostics(Uri uri)
    {
        CancellationTokenSource source;

        lock (_diagnosticsGate)
        {
            CancelPendingDiagnosticsNoLock(uri);
            source = new CancellationTokenSource();
            _pendingDiagnostics[uri] = source;
        }

        _ = PublishDiagnosticsAfterDelayAsync(uri, source.Token);
    }

    private void CancelPendingDiagnostics(Uri uri)
    {
        lock (_diagnosticsGate)
            CancelPendingDiagnosticsNoLock(uri);
    }

    private void CancelPendingDiagnosticsNoLock(Uri uri)
    {
        if (!_pendingDiagnostics.Remove(uri, out var existing))
            return;

        existing.Cancel();
        existing.Dispose();
    }

    private async Task PublishDiagnosticsAfterDelayAsync(Uri uri, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(_options.DiagnosticsDelay, cancellationToken);
            await PublishDiagnosticsAsync(uri, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Superseded by a newer keystroke.
        }
    }

    private async Task RepublishAllDiagnosticsAsync(CancellationToken cancellationToken)
    {
        foreach (var snapshot in _documents.GetSnapshots())
            await PublishDiagnosticsAsync(snapshot.Uri, cancellationToken);
    }

    private async Task PublishDiagnosticsAsync(Uri uri, CancellationToken cancellationToken)
    {
        try
        {
            var snapshot = await TryGetSnapshotAsync(uri, cancellationToken);
            if (snapshot is null)
                return;

            var document = snapshot.Value.Document;
            var text = document.Text;

            var diagnostics = CatalogError is { } catalogError
                ? await GetSyntaxOnlyDiagnosticsAsync(document, catalogError, cancellationToken)
                : await GetFullDiagnosticsAsync(document, cancellationToken);

            await SendDiagnosticsAsync(uri, snapshot.Value.Version, diagnostics);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await SafeLogAsync(MessageType.Error, $"Failed to compute diagnostics for {uri}: {ex}");
        }
    }

    private const string DiagnosticSource = @"nquery";
    private const string CatalogUnavailableCode = @"CatalogUnavailable";

    private static async Task<List<LspDiagnostic>> GetFullDiagnosticsAsync(Document document,
                                                                          CancellationToken cancellationToken)
    {
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
        var text = document.Text;
        var diagnostics = new List<LspDiagnostic>();

        // SemanticModel.GetDiagnostics() is binding diagnostics only -- Compilation combines it
        // with the syntax tree's privately -- so the two sets are gathered here. They are
        // disjoint, so concatenating cannot duplicate. Without this an unterminated string or
        // stray parenthesis produces no squiggle at all.
        foreach (var diagnostic in semanticModel.SyntaxTree.GetDiagnostics())
        {
            diagnostics.Add(new LspDiagnostic
            {
                Range = text.ToRange(diagnostic.Span),
                Severity = DiagnosticSeverity.Error,
                Code = diagnostic.DiagnosticId.ToString(),
                Source = DiagnosticSource,
                Message = diagnostic.Message
            });
        }

        // NQuery's Diagnostic carries no severity, so everything the compiler reports is an
        // error; CodeIssue is the only source that distinguishes levels today.
        foreach (var diagnostic in semanticModel.GetDiagnostics())
        {
            diagnostics.Add(new LspDiagnostic
            {
                Range = text.ToRange(diagnostic.Span),
                Severity = DiagnosticSeverity.Error,
                Code = diagnostic.DiagnosticId.ToString(),
                Source = DiagnosticSource,
                Message = diagnostic.Message
            });
        }

        foreach (var issue in document.Services.GetService<CodeIssueService>().GetIssues(document, cancellationToken))
        {
            diagnostics.Add(new LspDiagnostic
            {
                Range = text.ToRange(issue.Span),
                Severity = ToSeverity(issue.Kind),
                Source = DiagnosticSource,
                Message = GetMessage(issue),
                Tags = issue.Kind == CodeIssueKind.Unnecessary ? [DiagnosticTag.Unnecessary] : null
            });
        }

        return diagnostics;
    }

    // Without a catalog every table and column in the document is undeclared, so binding would
    // report dozens of errors that all describe one problem. Parsing needs no catalog, so syntax
    // errors are still real and still reported -- plus a single diagnostic naming the actual
    // cause.
    private static async Task<List<LspDiagnostic>> GetSyntaxOnlyDiagnosticsAsync(Document document,
                                                                                 string catalogError,
                                                                                 CancellationToken cancellationToken)
    {
        var syntaxTree = await document.GetSyntaxTreeAsync(cancellationToken);
        var text = document.Text;

        var diagnostics = new List<LspDiagnostic>
        {
            new()
            {
                Range = text.ToRange(GetFirstLineSpan(text)),
                Severity = DiagnosticSeverity.Error,
                Code = CatalogUnavailableCode,
                Source = DiagnosticSource,
                Message = $"The catalog is unavailable, so names cannot be resolved: {catalogError}"
            }
        };

        foreach (var diagnostic in syntaxTree.GetDiagnostics())
        {
            diagnostics.Add(new LspDiagnostic
            {
                Range = text.ToRange(diagnostic.Span),
                Severity = DiagnosticSeverity.Error,
                Code = diagnostic.DiagnosticId.ToString(),
                Source = DiagnosticSource,
                Message = diagnostic.Message
            });
        }

        return diagnostics;
    }

    // The problem belongs to the document rather than to any one span, and a zero-length range
    // renders as an invisible squiggle, so it is anchored to the first line.
    private static TextSpan GetFirstLineSpan(SourceText text)
    {
        return text.Lines.Count > 0
            ? text.Lines[0].Span
            : new TextSpan(0, 0);
    }

    private static DiagnosticSeverity ToSeverity(CodeIssueKind kind)
    {
        return kind switch
        {
            CodeIssueKind.Error => DiagnosticSeverity.Error,
            CodeIssueKind.Warning => DiagnosticSeverity.Warning,

            // "Unnecessary" is not a problem to fix, it is dead code to grey out.
            CodeIssueKind.Unnecessary => DiagnosticSeverity.Hint,
            _ => DiagnosticSeverity.Information
        };
    }

    // A CodeIssue may carry only actions (the description then lives on the action), but LSP
    // requires a message.
    private static string GetMessage(CodeIssue issue)
    {
        if (!string.IsNullOrEmpty(issue.Description))
            return issue.Description;

        return issue.Actions.Length > 0
            ? issue.Actions[0].Description
            : issue.Kind.ToString();
    }

    private Task SendDiagnosticsAsync(Uri uri, int? version, IReadOnlyList<LspDiagnostic> diagnostics)
    {
        var parameters = new PublishDiagnosticsParams
        {
            Uri = uri,
            Version = version,
            Diagnostics = diagnostics
        };

        return Rpc.NotifyWithParameterObjectAsync(Methods.TextDocumentPublishDiagnostics, parameters);
    }

    private async Task SafeLogAsync(MessageType type, string message)
    {
        try
        {
            await LogAsync(type, message);
        }
        catch (Exception)
        {
            // Connection is gone.
        }
    }
}
