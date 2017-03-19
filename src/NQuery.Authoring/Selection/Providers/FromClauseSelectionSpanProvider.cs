using NQuery.Syntax;

namespace NQuery.Authoring.Selection.Providers
{
    internal sealed class FromClauseSelectionSpanProvider : SeparatedSyntaxListSelectionSpanProvider<FromClauseSyntax, TableReferenceSyntax>
    {
        protected override SeparatedSyntaxList<TableReferenceSyntax> GetList(FromClauseSyntax node)
        {
            return node.TableReferences;
        }
    }
}