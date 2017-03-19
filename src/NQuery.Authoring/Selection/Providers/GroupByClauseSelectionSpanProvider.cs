using NQuery.Syntax;

namespace NQuery.Authoring.Selection.Providers
{
    internal sealed class GroupByClauseSelectionSpanProvider : SeparatedSyntaxListSelectionSpanProvider<GroupByClauseSyntax, GroupByColumnSyntax>
    {
        protected override SeparatedSyntaxList<GroupByColumnSyntax> GetList(GroupByClauseSyntax node)
        {
            return node.Columns;
        }
    }
}