using NQuery.Syntax;

namespace NQuery.Authoring.Selection.Providers
{
    internal sealed class OrderedQuerySelectionSpanProvider : SeparatedSyntaxListSelectionSpanProvider<OrderedQuerySyntax, OrderByColumnSyntax>
    {
        protected override SeparatedSyntaxList<OrderByColumnSyntax> GetList(OrderedQuerySyntax node)
        {
            return node.Columns;
        }
    }
}