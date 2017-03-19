using NQuery.Syntax;

namespace NQuery.Authoring.Selection.Providers
{
    internal sealed class CommonTableExpressionColumnNameListSelectionSpanProvider : SeparatedSyntaxListSelectionSpanProvider<CommonTableExpressionColumnNameListSyntax, CommonTableExpressionColumnNameSyntax>
    {
        protected override SeparatedSyntaxList<CommonTableExpressionColumnNameSyntax> GetList(CommonTableExpressionColumnNameListSyntax node)
        {
            return node.ColumnNames;
        }
    }
}