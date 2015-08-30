using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.Text;

namespace NQuery.Authoring.Outlining.Outliners
{
    public sealed class SingleLineCommentOutliner : SyntaxTokenOutliner
    {
        protected override IEnumerable<OutliningRegionSpan> FindRegions(SyntaxToken token)
        {
            var leadingSpans = GetConsecutiveCommentSpans(token.LeadingTrivia);
            var trailingSpans = GetConsecutiveCommentSpans(token.TrailingTrivia);
            return leadingSpans.Concat(trailingSpans);
        }

        private static IEnumerable<OutliningRegionSpan> GetConsecutiveCommentSpans(IEnumerable<SyntaxTrivia> trivias)
        {
            SyntaxTrivia firstComment = null;
            SyntaxTrivia lastComment = null;

            foreach (var trivia in trivias)
            {
                switch (trivia.Kind)
                {
                    case SyntaxKind.SingleLineCommentTrivia:
                        if (firstComment == null)
                            firstComment = trivia;
                        lastComment = trivia;
                        break;
                    case SyntaxKind.WhitespaceTrivia:
                    case SyntaxKind.EndOfLineTrivia:
                        // Ignore
                        break;
                    default:
                        if (firstComment != null)
                            yield return CreateRegionSpan(firstComment, lastComment);

                        firstComment = null;
                        lastComment = null;
                        break;
                }
            }

            if (firstComment != null)
                yield return CreateRegionSpan(firstComment, lastComment);
        }

        private static OutliningRegionSpan CreateRegionSpan(SyntaxTrivia firstComment, SyntaxTrivia lastComment)
        {
            var start = firstComment.Span.Start;
            var end = lastComment.Span.End;
            var span = TextSpan.FromBounds(start, end);
            var text = firstComment.Text + " ...";
            return new OutliningRegionSpan(span, text);
        }
    }
}