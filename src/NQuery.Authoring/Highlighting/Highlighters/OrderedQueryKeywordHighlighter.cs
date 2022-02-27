﻿using NQuery.Syntax;
using NQuery.Text;

namespace NQuery.Authoring.Highlighting.Highlighters
{
    internal sealed class OrderedQueryKeywordHighlighter : SelectQueryKeywordHighlighterBase<OrderedQuerySyntax>
    {
        private static IEnumerable<TextSpan> GetOrderByHighlights(OrderedQuerySyntax node)
        {
            yield return TextSpan.FromBounds(node.OrderKeyword.Span.Start,
                                             node.ByKeyword.Span.End);
        }

        protected override IEnumerable<TextSpan> GetHighlights(SemanticModel semanticModel, OrderedQuerySyntax node, int position)
        {
            var selectQuery = node.Query.DescendantNodesAndSelf()
                                  .SkipWhile(n => n is ParenthesizedQuerySyntax)
                                  .FirstOrDefault() as SelectQuerySyntax;

            if (selectQuery is null)
                return Enumerable.Empty<TextSpan>();

            var selectQueryHighlights = GetSelectQueryHighlights(selectQuery);
            var orderByHighlights = GetOrderByHighlights(node);
            return selectQueryHighlights.Concat(orderByHighlights);
        }
    }
}