using NQuery.Syntax;

namespace NQuery.Authoring.Selection.Providers
{
    internal sealed class CommonTableExpressionQuerySelectionSpanProvider : SeparatedSyntaxListSelectionSpanProvider<CommonTableExpressionQuerySyntax, CommonTableExpressionSyntax>
    {
        protected override SeparatedSyntaxList<CommonTableExpressionSyntax> GetList(CommonTableExpressionQuerySyntax node)
        {
            return node.CommonTableExpressions;
        }
    }
}