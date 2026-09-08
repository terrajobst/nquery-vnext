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
                    ? GetCompletedLineFilter(document.Text, parameters.Position.Line)
                    : GetJustClosedFilter(syntaxTree, document.Text.ToOffset(parameters.Position));

        if (keep is null)
            return null;

        return await FormatAsync(parameters.TextDocument.Uri, parameters.Options, keep, cancellationToken);
    }

    // The line the newline just ended, which is the one above the cursor.
    private static Func<TextChange, bool>? GetCompletedLineFilter(SourceText text, int cursorLine)
    {
        if (cursorLine <= 0 || cursorLine >= text.Lines.Count)
            return null;

        var line = text.Lines[cursorLine - 1].Span;

        return c =>
        {
            // Everything the line holds outright: its spacing, its casing, and a break the
            // formatter wants inside it. The gap after the last token is not one of these -- that
            // is where the newline just typed lives, and rendering it is how the formatter would
            // take it straight back.
            if (c.Span.Start >= line.Start && c.Span.End <= line.End)
                return true;

            // The line's own indentation, which sits in the gap in front of it and so starts on the
            // line before. An indent that came out wrong is much of what Enter is pressed to fix,
            // so this is worth reaching back for -- but only while it stays an indent. A
            // replacement with no newline left in it would pull the line up onto the previous one,
            // and moving text that is already placed is the opposite of what Enter asked for.
            return c.Span.Start < line.Start &&
                   c.Span.End >= line.Start &&
                   c.Span.End <= line.End &&
                   c.NewText.Contains('\n');
        };
    }

    // What the parenthesis the user just typed closes. The cursor sits immediately after it, so the
    // token on the left is that parenthesis -- unless it isn't, because a client may trigger on a
    // ')' inside a string or a comment, or against a document version where the edit has not landed
    // yet. Either way the answer is to format nothing rather than to guess at a construct.
    //
    // Formatting the parent node rather than the parenthesis pair is what makes the result useful:
    // the pair is just two characters, while the node is the argument list or subquery that was
    // being typed, and its layout is the thing that was left half-done.
    private static Func<TextChange, bool>? GetJustClosedFilter(SyntaxTree syntaxTree, int position)
    {
        var token = syntaxTree.Root.FindTokenOnLeft(position);

        if (token.Kind != SyntaxKind.RightParenthesisToken || token.IsMissing)
            return null;

        if (token.Span.End != position || token.Parent is not { } node)
            return null;

        // Strictly inside the construct: not the whitespace in front of it, and not the final
        // newline, which a construct ending the document would otherwise drag in.
        var span = node.Span;
        return c => c.Span.Start > span.Start && c.Span.End < span.End;
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
