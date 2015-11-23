using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using NQuery.Text;

namespace NQuery.Authoring.Commenting
{
    public static class Commenter
    {
        public static SyntaxTree ToggleSingleLineComment(this SyntaxTree syntaxTree, TextSpan textSpan)
        {
            if (syntaxTree == null)
                throw new ArgumentNullException(nameof(syntaxTree));

            var comments = syntaxTree.GetConsecutiveSingleLineComments(textSpan);
            return comments.IsDefaultOrEmpty
                    ? syntaxTree.CommentSingleLineComment(textSpan)
                    : syntaxTree.UncommentSingleLineComment(comments);
        }

        private static ImmutableArray<SyntaxTrivia> GetConsecutiveSingleLineComments(this SyntaxTree syntaxTree, TextSpan textSpan)
        {
            ImmutableArray<SyntaxTrivia> trivias;
            int startIndex;
            int endIndex;

            if (!syntaxTree.TryGetStartAndEndComment(textSpan, out trivias, out startIndex, out endIndex))
                return ImmutableArray<SyntaxTrivia>.Empty;

            var result = ImmutableArray.CreateBuilder<SyntaxTrivia>();

            // If we find any trivia between the comments that isn't a single
            // line comment or a line break, they they aren't consecutive.
            //
            // NOTE: We include the start and end trivias because we haven't
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
                        return ImmutableArray<SyntaxTrivia>.Empty;
                }
            }

            return result.ToImmutable();
        }

        private static SyntaxTree CommentSingleLineComment(this SyntaxTree syntaxTree, TextSpan textSpan)
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

        private static SyntaxTree UncommentSingleLineComment(this SyntaxTree syntaxTree, ImmutableArray<SyntaxTrivia> textSpan)
        {
            var changes = textSpan.Select(t => TextChange.ForDeletion(new TextSpan(t.Span.Start, 2)));
            return syntaxTree.WithChanges(changes);
        }

        public static SyntaxTree ToggleMultiLineComment(this SyntaxTree syntaxTree, TextSpan textSpan)
        {
            if (syntaxTree == null)
                throw new ArgumentNullException(nameof(syntaxTree));

            var comment = syntaxTree.GetMultiLineComment(textSpan);
            return comment != null
                ? syntaxTree.UncommentMultiLineComment(comment)
                : syntaxTree.CommentMultiLineComment(textSpan);
        }

        private static SyntaxTrivia GetMultiLineComment(this SyntaxTree syntaxTree, TextSpan textSpan)
        {
            ImmutableArray<SyntaxTrivia> trivias;
            int startIndex;
            int endIndex;

            if (!syntaxTree.TryGetStartAndEndComment(textSpan, out trivias, out startIndex, out endIndex))
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

        private static SyntaxTree CommentMultiLineComment(this SyntaxTree syntaxTree, TextSpan textSpan)
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

        private static SyntaxTree UncommentMultiLineComment(this SyntaxTree syntaxTree, SyntaxTrivia comment)
        {
            var changes = new List<TextChange>(2);

            changes.Add(TextChange.ForDeletion(new TextSpan(comment.Span.Start, 2)));

            if (comment.IsTerminated())
                changes.Add(TextChange.ForDeletion(new TextSpan(comment.Span.End - 2, 2)));

            return syntaxTree.WithChanges(changes);
        }

        private static bool TryGetStartAndEndComment(this SyntaxTree syntaxTree, TextSpan textSpan, out ImmutableArray<SyntaxTrivia> trivias, out int startIndex, out int endIndex)
        {
            startIndex = -1;
            endIndex = -1;
            trivias = ImmutableArray<SyntaxTrivia>.Empty;

            // Find the associated token

            var startToken = syntaxTree.Root.FindToken(textSpan.Start, true);
            var endToken = syntaxTree.Root.FindToken(textSpan.End, true)
                                          .GetPreviousIfCurrentContainsOrTouchesPosition(textSpan.End);

            // If span is over different tokens, then the trivias cannot be
            // from the same collection.

            if (startToken != endToken)
                return false;

            var token = startToken;

            // In order for the trivias to come from the same collection they
            // must both be leading or both be trailing.

            var spanIsBeforeToken = textSpan.End <= token.Span.Start;
            var spanIsAfterToken = textSpan.Start >= token.Span.End;
            if (!spanIsBeforeToken && !spanIsAfterToken)
                return false;

            // Select trivia collection

            trivias = spanIsBeforeToken ? token.LeadingTrivia : token.TrailingTrivia;

            // Find the indices of the trivias that contain the start and end positions.

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
}