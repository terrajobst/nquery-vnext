using NQuery.Syntax;

namespace NQuery.Authoring.Selection.Providers
{
    internal sealed class SelectClauseSelectionSpanProvider : SeparatedSyntaxListSelectionSpanProvider<SelectClauseSyntax, SelectColumnSyntax>
    {
        protected override SeparatedSyntaxList<SelectColumnSyntax> GetList(SelectClauseSyntax node)
        {
            return node.Columns;
        }
    }
}