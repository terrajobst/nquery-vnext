using NQuery.Authoring.Formatting;
using NQuery.Authoring.LanguageServer.Mapping;
using NQuery.Authoring.LanguageServer.Protocol;
using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Text;

using StreamJsonRpc;

using LspFormattingOptions = NQuery.Authoring.LanguageServer.Protocol.FormattingOptions;

namespace NQuery.Authoring.LanguageServer.Server;

internal sealed partial class LanguageServerTarget
{
    [JsonRpcMethod(Methods.TextDocumentFormatting, UseSingleObjectParameterDeserialization = true)]
    public async Task<TextEdit[]?> FormattingAsync(DocumentFormattingParams parameters, CancellationToken cancellationToken)
    {
        ThrowIfNull(parameters);

        return await FormatAsync(parameters.TextDocument.Uri, parameters.Options, keep: null, cancellationToken);
    }

    [JsonRpcMethod(Methods.TextDocumentRangeFormatting, UseSingleObjectParameterDeserialization = true)]
    public async Task<TextEdit[]?> RangeFormattingAsync(DocumentRangeFormattingParams parameters, CancellationToken cancellationToken)
    {
        ThrowIfNull(parameters);

        var snapshot = await TryGetSnapshotAsync(parameters.TextDocument.Uri, cancellationToken);
        if (snapshot is null)
            return null;

        var range = snapshot.Value.Document.Text.ToTextSpan(parameters.Range);
        return await FormatAsync(parameters.TextDocument.Uri, parameters.Options, range.IntersectsWith, cancellationToken);
    }

    [JsonRpcMethod(Methods.TextDocumentOnTypeFormatting, UseSingleObjectParameterDeserialization = true)]
    public async Task<TextEdit[]?> OnTypeFormattingAsync(DocumentOnTypeFormattingParams parameters, CancellationToken cancellationToken)
    {
        ThrowIfNull(parameters);

        var snapshot = await TryGetSnapshotAsync(parameters.TextDocument.Uri, cancellationToken);
        if (snapshot is null)
            return null;

        var document = snapshot.Value.Document;
        var syntaxTree = await document.GetSyntaxTreeAsync(cancellationToken);
        var position = document.Text.ToOffset(parameters.Position);

        var construct = GetJustClosedSpan(syntaxTree, position);
        if (construct is null)
            return null;

        // Strictly inside, where range formatting takes anything that so much as touches its span.
        // Nobody asked for this one -- it is a keystroke, not a command -- so it has no business
        // reaching past the construct: not to the whitespace in front of it, and not to the final
        // newline, which a construct ending the document would otherwise drag in.
        var span = construct.Value;
        return await FormatAsync(parameters.TextDocument.Uri,
                                 parameters.Options,
                                 c => c.Start > span.Start && c.End < span.End,
                                 cancellationToken);
    }

    // What the parenthesis the user just typed closes. The cursor sits immediately after it, so the
    // token on the left is that parenthesis -- unless it isn't, because a client may trigger on a
    // ')' inside a string or a comment, or against a document version where the edit has not landed
    // yet. Either way the answer is to format nothing rather than to guess at a construct.
    //
    // Formatting the parent node rather than the parenthesis pair is what makes the result useful:
    // the pair is just two characters, while the node is the argument list or subquery that was
    // being typed, and its layout is the thing that was left half-done.
    private static TextSpan? GetJustClosedSpan(SyntaxTree syntaxTree, int position)
    {
        var token = syntaxTree.Root.FindTokenOnLeft(position);

        if (token.Kind != SyntaxKind.RightParenthesisToken || token.IsMissing)
            return null;

        if (token.Span.End != position)
            return null;

        return token.Parent?.Span;
    }

    // Formatting is syntactic, so this needs the syntax tree and nothing else -- which is what keeps
    // it working in a document whose catalog failed to load.
    //
    // The document is always formatted whole, because what a line is indented to depends on
    // everything enclosing it; keep then picks out the changes the caller actually asked about. The
    // corollary is that those changes assume the rest of the document was formatted too, so in one
    // that isn't, a kept change can carry a column computed for text nobody is going to rewrite.
    // That is the standing bargain of formatting a part of something, and it costs nothing in the
    // documents this is used on, which format on save.
    private async Task<TextEdit[]?> FormatAsync(Uri uri, LspFormattingOptions clientOptions, Func<TextSpan, bool>? keep, CancellationToken cancellationToken)
    {
        var snapshot = await TryGetSnapshotAsync(uri, cancellationToken);
        if (snapshot is null)
            return null;

        var document = snapshot.Value.Document;
        await document.GetSyntaxTreeAsync(cancellationToken);

        var text = document.Text;
        var service = document.Services.GetService<FormattingService>();

        // Whatever the document's own .editorconfig says wins over both of these, which is what
        // asking the service rather than using the baseline directly is for.
        var baseline = GetFormattingOptions(clientOptions, text);
        var options = service.GetOptions(document, baseline, cancellationToken);

        var changes = service.GetChanges(document, options, cancellationToken);

        return [.. changes.Where(c => keep is null || keep(c.Span))
                          .Select(c => new TextEdit { Range = text.ToRange(c.Span), NewText = c.NewText })];
    }

    // The style is server policy; the client only has a say in the handful of values LSP defines,
    // which are the user's editor settings for this document and so win over the defaults. This is
    // only the baseline: a checked-in .editorconfig belongs to everyone working on the file and
    // outranks both.
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
    // a file mixed line endings the first time it is formatted. An end_of_line in an .editorconfig
    // overrides this, because that one is a decision somebody wrote down.
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
