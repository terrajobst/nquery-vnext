using System.Collections.Immutable;
using System.Text;

using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Syntax;
using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.Formatting;

// Turns a tree plus options into text changes.
//
// The tree is a red-only view over its text -- a token's text is a slice of the document, and there
// is no factory for building new nodes -- so formatting can't mean printing a rewritten tree. It
// means rewriting the whitespace between tokens, plus the few token texts the casing and identifier
// passes change. Both kinds of change land on disjoint spans, so they compose into one edit.
internal sealed class Formatter
{
    private readonly FormattingOptions _options;
    private readonly SourceText _text;
    private readonly ImmutableArray<SyntaxToken> _tokens;
    private readonly ImmutableArray<string> _tokenTexts;
    private readonly Dictionary<SyntaxToken, Gap> _gaps;
    private readonly ImmutableArray<GapGroup> _groups;
    private readonly ImmutableArray<ImmutableArray<int>> _groupsStartingAt;
    private readonly bool?[] _isFlat;
    private readonly bool _isExpression;

    private readonly List<TextChange> _changes = [];

    private int _column;
    private int _lineIndent;

    private Formatter(SyntaxTree syntaxTree, FormattingOptions options)
    {
        _options = options;
        _text = syntaxTree.Text;
        _tokens = [.. syntaxTree.Root.DescendantTokens()];

        var indexOf = new Dictionary<SyntaxToken, int>();
        for (var i = 0; i < _tokens.Length; i++)
            indexOf[_tokens[i]] = i;

        _tokenTexts = [.. _tokens.Select(t => TokenTextRules.GetText(t, options))];

        var (gaps, groups) = LayoutWalker.Compute(syntaxTree.Root, options, _tokens, indexOf);
        _gaps = gaps;
        _groups = groups;
        _isFlat = new bool?[groups.Length];
        _isExpression = syntaxTree.Root.Root is ExpressionSyntax;

        var startingAt = new List<int>[_tokens.Length];
        for (var i = 0; i < groups.Length; i++)
        {
            var start = groups[i].Start;
            (startingAt[start] ??= []).Add(i);
        }

        _groupsStartingAt = [.. startingAt.Select(g => g is null ? ImmutableArray<int>.Empty : [.. g])];
    }

    public static ImmutableArray<TextChange> GetChanges(SyntaxTree syntaxTree, FormattingOptions options)
    {
        var formatter = new Formatter(syntaxTree, options);
        formatter.Run();
        return [.. formatter._changes];
    }

    private void Run()
    {
        for (var i = 0; i < _tokens.Length; i++)
        {
            RenderGap(i);

            // A group's decision needs the column its first token starts at, which is only known
            // once everything before it has been rendered -- hence outermost first, here, rather
            // than in a pass of its own.
            foreach (var group in _groupsStartingAt[i])
                _isFlat[group] = IsFlat(group);

            RenderToken(i);
        }
    }

    // A zero-length insertion sitting immediately in front of another change -- a line break before
    // a keyword the casing pass also rewrites -- is indistinguishable from an overlapping edit to
    // anything that validates changes by position, so the two go out as one.
    private void AddChange(TextSpan span, string newText)
    {
        if (_changes.Count > 0)
        {
            var last = _changes[_changes.Count - 1];
            if (last.Span.Length == 0 && last.Span.Start == span.Start)
            {
                _changes[_changes.Count - 1] = TextChange.ForReplacement(span, last.NewText + newText);
                return;
            }
        }

        _changes.Add(TextChange.ForReplacement(span, newText));
    }

    // -- Tokens -------------------------------------------------------------------------------

    private void RenderToken(int index)
    {
        var token = _tokens[index];
        var text = _tokenTexts[index];

        if (!string.Equals(text, token.Text, StringComparison.Ordinal))
            AddChange(token.Span, text);

        Advance(text, isGap: false);
    }

    // -- Gaps ---------------------------------------------------------------------------------

    private void RenderGap(int index)
    {
        var current = _tokens[index];
        var start = index == 0 ? 0 : _tokens[index - 1].Span.End;
        var end = current.Span.Start;

        // Missing tokens have zero-length spans and can sit anywhere, including on top of their
        // neighbors; there is no gap to speak of between them.
        if (end < start)
            return;

        var span = TextSpan.FromBounds(start, end);
        var original = _text.GetText(span);

        var newText = PreserveVerbatim(index)
                        ? original
                        : RenderGapText(index, span, original);

        if (!string.Equals(newText, original, StringComparison.Ordinal))
            AddChange(span, newText);

        Advance(newText, isGap: true);
    }

    // Error recovery puts the tokens it couldn't place into trivia and leaves zero-length tokens
    // where it expected something. Neither has a reliable position, so a gap touching one is copied
    // through: the document keeps formatting everywhere else while the broken region is left alone.
    private bool PreserveVerbatim(int index)
    {
        var current = _tokens[index];
        if (current.IsMissing)
            return true;

        if (index > 0 && _tokens[index - 1].IsMissing)
            return true;

        // An unterminated literal runs to the end of the text, so anything put after it is swallowed
        // by it -- the final newline included.
        if (index > 0 && !_tokens[index - 1].IsTerminated())
            return true;

        return HasSkippedTokens(current.LeadingTrivia) ||
               index > 0 && HasSkippedTokens(_tokens[index - 1].TrailingTrivia);
    }

    private static bool HasSkippedTokens(ImmutableArray<SyntaxTrivia> trivia)
    {
        foreach (var t in trivia)
        {
            if (t.Kind == SyntaxKind.SkippedTokensTrivia)
                return true;
        }

        return false;
    }

    private string RenderGapText(int index, TextSpan span, string original)
    {
        var kind = Resolve(index);
        var column = GetGap(index).Column;
        var comments = GetComments(index, span);

        if (comments.Count == 0)
            return RenderWhitespace(kind, column, CountBlankLines(original));

        return RenderComments(comments, span, kind, column, isDocumentStart: index == 0);
    }

    private string RenderWhitespace(GapKind kind, int column, int blankLines)
    {
        switch (kind)
        {
            case GapKind.None:
                return string.Empty;
            case GapKind.Space:
                return @" ";
            case GapKind.Pad:
                // Past the column already: the alignment is lost either way, and swallowing the
                // separator would join two tokens.
                return column > _column ? new string(' ', column - _column) : @" ";
            case GapKind.Line:
                var builder = new StringBuilder();
                for (var i = 0; i <= blankLines; i++)
                    builder.Append(_options.NewLine);
                builder.Append(GetIndent(column));
                return builder.ToString();
            default:
                throw ExceptionBuilder.UnexpectedValue(kind);
        }
    }

    // A comment can't be moved without changing what it comments on, so the gap is rendered around
    // it rather than replaced: the whitespace on each side is normalized, and whether the comment
    // was trailing or on a line of its own is preserved. A single line comment always ends its
    // line -- anything else would swallow the token after it.
    private string RenderComments(List<SyntaxTrivia> comments, TextSpan span, GapKind kind, int column, bool isDocumentStart)
    {
        var builder = new StringBuilder();
        var commentColumn = kind == GapKind.Line ? column : _lineIndent;
        var position = span.Start;

        for (var i = 0; i < comments.Count; i++)
        {
            var comment = comments[i];
            var before = _text.GetText(TextSpan.FromBounds(position, comment.Span.Start));
            var startsLine = before.Contains('\n') || i > 0;

            if (i == 0 && !startsLine)
            {
                // Nothing precedes the first token of the document, so there is nothing to separate
                // a comment in front of it from.
                if (!isDocumentStart)
                    builder.Append(' ');
            }
            else if (startsLine)
                builder.Append(_options.NewLine).Append(GetIndent(commentColumn));
            else
                builder.Append(' ');

            // A multi line comment carries its own breaks, and they are breaks in this file like
            // any other. Its interior indentation is left alone: that is the author's layout.
            builder.Append(LineBreakRules.Normalize(_text.GetText(comment.Span), _options.NewLine));
            position = comment.Span.End;
        }

        var after = _text.GetText(TextSpan.FromBounds(position, span.End));
        var last = comments[comments.Count - 1];
        var mustBreak = last.Kind == SyntaxKind.SingleLineCommentTrivia ||
                        after.Contains('\n') ||
                        kind == GapKind.Line;

        if (mustBreak)
            builder.Append(_options.NewLine).Append(GetIndent(kind == GapKind.Line ? column : _lineIndent));
        else
            builder.Append(' ');

        return builder.ToString();
    }

    private List<SyntaxTrivia> GetComments(int index, TextSpan span)
    {
        var result = new List<SyntaxTrivia>();

        if (index > 0)
            AddComments(result, _tokens[index - 1].TrailingTrivia, span);

        AddComments(result, _tokens[index].LeadingTrivia, span);

        return result;
    }

    private static void AddComments(List<SyntaxTrivia> target, ImmutableArray<SyntaxTrivia> trivia, TextSpan span)
    {
        foreach (var t in trivia)
        {
            if (t.Kind.IsComment() && span.Contains(t.Span.Start))
                target.Add(t);
        }
    }

    private int CountBlankLines(string original)
    {
        var newLines = 0;
        foreach (var c in original)
        {
            if (c == '\n')
                newLines++;
        }

        return Math.Min(_options.MaxBlankLines, Math.Max(0, newLines - 1));
    }

    // -- Group resolution ---------------------------------------------------------------------

    private GapKind Resolve(int index)
    {
        var current = _tokens[index];

        // The document's first token, and the end of the file, aren't between anything.
        if (index == 0)
            return GapKind.None;

        if (current.Kind == SyntaxKind.EndOfFileToken)
            // An expression document is one expression rather than a file, and a trailing newline in
            // an expression box is just a stray character.
            return _options.InsertFinalNewline && !_isExpression ? GapKind.Line : GapKind.None;

        var gap = GetGap(index);
        if (gap.Kind != GapKind.SoftLine)
            return gap.Kind;

        var isFlat = gap.Group >= 0 && _isFlat[gap.Group] == true;

        // Flat means "as if the walker had never asked for a break", which is the spacing rules'
        // answer -- not a space, or COUNT( * ) is what comes out.
        return isFlat
                ? SpacingRules.GetGap(_tokens[index - 1], current).Kind
                : GapKind.Line;
    }

    private Gap GetGap(int index)
    {
        var current = _tokens[index];

        if (_gaps.TryGetValue(current, out var gap))
            return gap;

        if (index == 0 || current.Kind == SyntaxKind.EndOfFileToken)
            return new Gap(GapKind.None);

        return SpacingRules.GetGap(_tokens[index - 1], current);
    }

    private bool IsFlat(int group)
    {
        var range = _groups[group];

        // Everything inside a flat group is flat, whatever it would have decided on its own.
        if (range.Parent >= 0 && _isFlat[range.Parent] == true)
            return true;

        var width = _column;

        for (var i = range.Start; i < range.End && i < _tokens.Length; i++)
        {
            if (i > range.Start)
            {
                var gap = GetGap(i);
                var kind = gap.Kind == GapKind.SoftLine
                            ? SpacingRules.GetGap(_tokens[i - 1], _tokens[i]).Kind
                            : gap.Kind;

                // A break the group can't get rid of, or a comment that forces one, means the group
                // was never going to fit on one line no matter how wide the budget is.
                if (kind == GapKind.Line || HasComment(i))
                    return false;

                width += kind == GapKind.None ? 0 : 1;
            }

            width += _tokenTexts[i].Length;

            if (_options.MaxLineLength > 0 && width > _options.MaxLineLength)
                return false;
        }

        return true;
    }

    private bool HasComment(int index)
    {
        foreach (var t in _tokens[index].LeadingTrivia)
        {
            if (t.Kind.IsComment())
                return true;
        }

        if (index > 0)
        {
            foreach (var t in _tokens[index - 1].TrailingTrivia)
            {
                if (t.Kind.IsComment())
                    return true;
            }
        }

        return false;
    }

    // -- Column tracking ----------------------------------------------------------------------

    private string GetIndent(int column)
    {
        if (column <= 0)
            return string.Empty;

        if (!_options.UseTabs)
            return new string(' ', column);

        var tabs = column / _options.IndentSize;
        var spaces = column % _options.IndentSize;
        return new string('\t', tabs) + new string(' ', spaces);
    }

    private void Advance(string text, bool isGap)
    {
        var index = text.LastIndexOf('\n');

        if (index < 0)
        {
            _column += text.Length;
            return;
        }

        _column = text.Length - index - 1;

        if (isGap)
            _lineIndent = _column;
    }
}
