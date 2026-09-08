using NQuery.Authoring.Formatting;
using NQuery.Authoring.LanguageServer.Mapping;
using NQuery.Authoring.LanguageServer.Protocol;
using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Syntax;
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
        return await FormatAsync(parameters.TextDocument.Uri, parameters.Options, c => range.IntersectsWith(c.Span), cancellationToken);
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

        // Nobody asked for any of this -- it is a keystroke, not a command -- so each trigger says
        // exactly which changes it will take, and range formatting's "anything that touches the
        // span" is far too generous for either of them.
        var keep = parameters.Ch.Contains('\n')
                    ? GetCompletedLineFilter(syntaxTree, document.Text, parameters.Position.Line)
                    : GetJustClosedFilter(syntaxTree, document.Text.ToOffset(parameters.Position));

        if (keep is null)
            return null;

        return await FormatAsync(parameters.TextDocument.Uri, parameters.Options, keep, cancellationToken);
    }

    // What the completed line finished: the largest node that ends on it. Walking up from the last
    // token on the line, rather than looking at the line's own text, is what makes this general --
    // a CASE closing on the line, a clause, a whole query, all fall out of the same walk, and
    // nothing here has to know which constructs exist.
    //
    // The line is only how the node is found; it is not the extent of the work, which is the point:
    // a construct that closes on this line was laid out across the ones above it too.
    //
    // The region is the node's span rather than its full span. Full span looks like the better
    // answer and isn't, because of where the lexer splits trivia: a line's ending goes to the token
    // it follows, so a node's leading trivia is only the indent after that newline, while the
    // formatter's change for that indent is the whole gap spanning the newline -- which starts
    // before the full span and is rejected by either. Full span reaches further at the other end
    // only, over the newline that was just typed, and admits exactly one change: the gap after the
    // construct, which is that newline. Excluding it costs nothing and spares a rule protecting it.
    private static Func<TextChange, bool>? GetCompletedLineFilter(SyntaxTree syntaxTree, SourceText text, int cursorLine)
    {
        if (cursorLine <= 0 || cursorLine >= text.Lines.Count)
            return null;

        var line = text.Lines[cursorLine - 1].Span;
        var token = syntaxTree.Root.FindTokenOnLeft(line.End);

        // A blank line, or one carrying nothing but trivia, finished nothing.
        if (token.IsMissing || token.Span.End <= line.Start)
            return null;

        SyntaxNode? node = null;
        for (var candidate = token.Parent; candidate is not null && candidate.Span.End <= line.End; candidate = candidate.Parent)
            node = candidate;

        // Every node the last token belongs to runs on past this line, so the line ended in the
        // middle of all of them and finished nothing. Leaving it alone is not just the conservative
        // answer, it is the correct one: formatting a construct that is still being typed would
        // pull the rest of it up onto this line, newline and all.
        if (node is null)
            return null;

        return Inside(node.Span);
    }

    private static Func<TextChange, bool>? GetJustClosedFilter(SyntaxTree syntaxTree, int position)
    {
        var token = syntaxTree.Root.FindTokenOnLeft(position);

        if (token.Kind != SyntaxKind.RightParenthesisToken || token.IsMissing)
            return null;

        if (token.Span.End != position || token.Parent is not { } node)
            return null;

        return Inside(node.Span);
    }

    // The construct's own text and nothing on either side of it: not the whitespace in front of it,
    // not the final newline that a construct ending the document would otherwise drag in, and --
    // when a keystroke put the cursor past it -- not the newline that was just typed either.
    //
    // Containment rather than a strictly-inside comparison, because the first and last tokens are
    // as much a part of the construct as anything between them. Rewriting a token's text is a
    // change spanning exactly that token, so a strict comparison drops both ends -- and a lower
    // case `case ... end` is precisely the pair it would leave standing.
    //
    // What containment lets back in is a gap that happens to be empty sitting on a boundary, which
    // is a change to what is beside the construct rather than to it. The final newline is one of
    // those, so they go by position instead.
    private static Func<TextChange, bool> Inside(TextSpan span)
    {
        return c => c.Span.Start >= span.Start &&
                    c.Span.End <= span.End &&
                    (c.Span.Length > 0 || c.Span.Start != span.Start && c.Span.Start != span.End);
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
    private async Task<TextEdit[]?> FormatAsync(Uri uri, LspFormattingOptions clientOptions, Func<TextChange, bool>? keep, CancellationToken cancellationToken)
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

        return [.. changes.Where(c => keep is null || keep(c))
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
