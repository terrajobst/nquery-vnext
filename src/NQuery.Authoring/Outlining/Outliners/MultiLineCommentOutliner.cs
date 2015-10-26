using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.Text;

namespace NQuery.Authoring.Outlining.Outliners
{
    internal sealed class MultiLineCommentOutliner : SyntaxTokenOutliner
    {
        protected override IEnumerable<OutliningRegionSpan> FindRegions(SyntaxToken token)
        {
            var sourceText = token.Parent.SyntaxTree.Text;
            var trivias = token.LeadingTrivia.Concat(token.TrailingTrivia);
            var comments = trivias.Where(t => t.Kind == SyntaxKind.MultiLineCommentTrivia);

            return from comment in comments
                   let commentStart = comment.Span.Start
                   let line = sourceText.GetLineFromPosition(commentStart)
                   let firstLineEnd = line.Span.End
                   let firstLineSpan = TextSpan.FromBounds(commentStart, firstLineEnd)
                   let text = sourceText.GetText(firstLineSpan) + " ..."
                   select new OutliningRegionSpan(comment.Span, text);
        }
    }
}