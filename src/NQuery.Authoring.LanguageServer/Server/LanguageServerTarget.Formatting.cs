using NQuery.Authoring.Formatting;
using NQuery.Authoring.LanguageServer.Mapping;
using NQuery.Authoring.LanguageServer.Protocol;
using NQuery.CodeAnalysis.Text;

using StreamJsonRpc;

using LspFormattingOptions = NQuery.Authoring.LanguageServer.Protocol.FormattingOptions;
using Range = NQuery.Authoring.LanguageServer.Protocol.Range;

namespace NQuery.Authoring.LanguageServer.Server;

internal sealed partial class LanguageServerTarget
{
    [JsonRpcMethod(Methods.TextDocumentFormatting, UseSingleObjectParameterDeserialization = true)]
    public async Task<TextEdit[]?> FormattingAsync(DocumentFormattingParams parameters, CancellationToken cancellationToken)
    {
        ThrowIfNull(parameters);

        return await FormatAsync(parameters.TextDocument.Uri, range: null, parameters.Options, cancellationToken);
    }

    [JsonRpcMethod(Methods.TextDocumentRangeFormatting, UseSingleObjectParameterDeserialization = true)]
    public async Task<TextEdit[]?> RangeFormattingAsync(DocumentRangeFormattingParams parameters, CancellationToken cancellationToken)
    {
        ThrowIfNull(parameters);

        return await FormatAsync(parameters.TextDocument.Uri, parameters.Range, parameters.Options, cancellationToken);
    }

    // Formatting is syntactic, so this needs the syntax tree and nothing else -- which is what keeps
    // it working in a document whose catalog failed to load.
    private async Task<TextEdit[]?> FormatAsync(Uri uri, Range? range, LspFormattingOptions clientOptions, CancellationToken cancellationToken)
    {
        var snapshot = await TryGetSnapshotAsync(uri, cancellationToken);
        if (snapshot is null)
            return null;

        var document = snapshot.Value.Document;
        await document.GetSyntaxTreeAsync(cancellationToken);

        var text = document.Text;
        var options = GetFormattingOptions(clientOptions, text);
        var service = document.Services.GetService<FormattingService>();

        var changes = range is null
                        ? service.GetChanges(document, options, cancellationToken)
                        : service.GetChanges(document, text.ToTextSpan(range), options, cancellationToken);

        return [.. changes.Select(c => new TextEdit { Range = text.ToRange(c.Span), NewText = c.NewText })];
    }

    // The style is server policy; the client only has a say in the handful of values LSP defines,
    // which are the user's editor settings for this document and so win over the defaults.
    private Formatting.FormattingOptions GetFormattingOptions(LspFormattingOptions clientOptions, SourceText text)
    {
        var options = _options.FormattingOptions;

        return options with
        {
            IndentSize = clientOptions.TabSize > 0 ? clientOptions.TabSize : options.IndentSize,
            UseTabs = !clientOptions.InsertSpaces,
            InsertFinalNewline = clientOptions.InsertFinalNewline ?? options.InsertFinalNewline,
            NewLine = GetNewLine(text)
        };
    }

    // Match whatever the document already uses rather than the server's platform, which would give
    // a file mixed line endings the first time it is formatted.
    private static string GetNewLine(SourceText text)
    {
        foreach (var line in text.Lines)
        {
            if (line.Span.End < line.SpanIncludingLineBreak.End)
                return text.GetText(TextSpan.FromBounds(line.Span.End, line.SpanIncludingLineBreak.End));
        }

        return Environment.NewLine;
    }
}
