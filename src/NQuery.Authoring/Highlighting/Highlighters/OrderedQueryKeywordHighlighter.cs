using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Syntax;
using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.Highlighting.Highlighters;

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
            return [];

        var selectQueryHighlights = GetSelectQueryHighlights(selectQuery);
        var orderByHighlights = GetOrderByHighlights(node);
        return selectQueryHighlights.Concat(orderByHighlights);
    }
}
