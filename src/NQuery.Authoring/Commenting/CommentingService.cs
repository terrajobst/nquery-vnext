using System.Collections.Immutable;

using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.Commenting;

// Document in, Document out. The rules below produce a new syntax tree, but a caller wanting to
// apply the result needs text, and every host that applies edits works in documents.
public sealed class CommentingService
{
    internal CommentingService()
    {
    }

    public Document ToggleSingleLineComment(DocumentView view, CancellationToken cancellationToken = default)
    {
        ThrowIfNull(view);

        var syntaxTree = view.Document.GetSyntaxTree(cancellationToken);
        var textSpan = view.Selection;

        var comments = GetConsecutiveSingleLineComments(syntaxTree, textSpan);
        var toggled = comments.IsDefaultOrEmpty
                        ? CommentSingleLineComment(syntaxTree, textSpan)
                        : UncommentSingleLineComment(syntaxTree, comments);

        return view.Document.WithText(toggled.Text);
    }

    public Document ToggleMultiLineComment(DocumentView view, CancellationToken cancellationToken = default)
    {
        ThrowIfNull(view);

        var syntaxTree = view.Document.GetSyntaxTree(cancellationToken);
        var textSpan = view.Selection;

        var comment = GetMultiLineComment(syntaxTree, textSpan);
        var toggled = comment is not null
                        ? UncommentMultiLineComment(syntaxTree, comment)
                        : CommentMultiLineComment(syntaxTree, textSpan);

        return view.Document.WithText(toggled.Text);
    }

    private static ImmutableArray<SyntaxTrivia> GetConsecutiveSingleLineComments(SyntaxTree syntaxTree, TextSpan textSpan)
    {
        if (!TryGetStartAndEndComment(syntaxTree, textSpan, out var trivias, out var startIndex, out var endIndex))
            return [];

        var result = ImmutableArray.CreateBuilder<SyntaxTrivia>();

        // If we find any trivia between the comments that isn't a single
        // line comment or a line break, they aren't consecutive.
        //
        // NOTE: We include the start and end trivia because we haven't
        //       yet verified whether they are actually single line comments.

        for (var i = startIndex; i <= endIndex; i++)
        {
            switch (trivias[i].Kind)
            {
                case SyntaxKind.SingleLineCommentTrivia:
                    result.Add(trivias[i]);
                    break;
                case SyntaxKind.EndOfLineTrivia:
                    // Ignore
                    break;
                default:
                    return [];
            }
        }

        return result.ToImmutable();
    }

    private static SyntaxTree CommentSingleLineComment(SyntaxTree syntaxTree, TextSpan textSpan)
    {
        var text = syntaxTree.Text;
        var startLine = text.GetLineNumberFromPosition(textSpan.Start);
        var endLine = text.GetLineNumberFromPosition(textSpan.End);
        var lineCount = endLine - startLine + 1;

        var changes = Enumerable.Range(startLine, lineCount)
                                .Select(i => text.Lines[i])
                                .Select(l => TextChange.ForInsertion(l.Span.Start, @"--"));

        return syntaxTree.WithChanges(changes);
    }

    private static SyntaxTree UncommentSingleLineComment(SyntaxTree syntaxTree, ImmutableArray<SyntaxTrivia> comments)
    {
        var changes = comments.Select(t => TextChange.ForDeletion(new TextSpan(t.Span.Start, 2)));
        return syntaxTree.WithChanges(changes);
    }

    private static SyntaxTrivia? GetMultiLineComment(SyntaxTree syntaxTree, TextSpan textSpan)
    {
        if (!TryGetStartAndEndComment(syntaxTree, textSpan, out var trivias, out var startIndex, out var endIndex))
            return null;

        // Is it a single comment?

        if (startIndex != endIndex)
            return null;

        var comment = trivias[startIndex];

        // OK, it's a single comment. Now let's see whether it's actually
        // a multi line comment.

        return comment.Kind == SyntaxKind.MultiLineCommentTrivia
                ? comment
                : null;
    }

    private static SyntaxTree CommentMultiLineComment(SyntaxTree syntaxTree, TextSpan textSpan)
    {
        var empty = new[]
        {
            TextChange.ForInsertion(textSpan.Start, @"/**/"),
        };

        var surround = new[]
        {
            TextChange.ForInsertion(textSpan.Start, @"/*"),
            TextChange.ForInsertion(textSpan.End, @"*/")
        };

        var changes = textSpan.Length == 0 ? empty : surround;

        return syntaxTree.WithChanges(changes);
    }

    private static SyntaxTree UncommentMultiLineComment(SyntaxTree syntaxTree, SyntaxTrivia comment)
    {
        var changes = new List<TextChange>(2);

        changes.Add(TextChange.ForDeletion(new TextSpan(comment.Span.Start, 2)));

        if (comment.IsTerminated())
            changes.Add(TextChange.ForDeletion(new TextSpan(comment.Span.End - 2, 2)));

        return syntaxTree.WithChanges(changes);
    }

    private static bool TryGetStartAndEndComment(SyntaxTree syntaxTree, TextSpan textSpan, out ImmutableArray<SyntaxTrivia> trivias, out int startIndex, out int endIndex)
    {
        startIndex = -1;
        endIndex = -1;
        trivias = [];

        // Find the associated token

        var startToken = syntaxTree.Root.FindToken(textSpan.Start, true);
        var endToken = syntaxTree.Root.FindToken(textSpan.End, true)
                                      .GetPreviousIfCurrentContainsOrTouchesPosition(textSpan.End);

        // If span is over different tokens, then the trivia cannot be
        // from the same collection.

        if (startToken != endToken)
            return false;

        var token = startToken;

        // In order for the trivia to come from the same collection they
        // must both be leading or both be trailing.

        var spanIsBeforeToken = textSpan.End <= token.Span.Start;
        var spanIsAfterToken = textSpan.Start >= token.Span.End;
        if (!spanIsBeforeToken && !spanIsAfterToken)
            return false;

        // Select trivia collection

        trivias = spanIsBeforeToken ? token.LeadingTrivia : token.TrailingTrivia;

        // Find the indices of the trivia that contain the start and end positions.

        startIndex = FindCommentIndex(trivias, textSpan.Start);
        endIndex = FindCommentIndex(trivias, textSpan.End);

        if (startIndex < 0 || endIndex < 0)
            return false;

        return true;
    }

    private static int FindCommentIndex(ImmutableArray<SyntaxTrivia> trivias, int position)
    {
        for (var i = 0; i < trivias.Length; i++)
        {
            var trivia = trivias[i];
            if (trivia.Kind.IsComment() && trivia.Span.ContainsOrTouches(position))
                return i;
        }

        return -1;
    }
}
