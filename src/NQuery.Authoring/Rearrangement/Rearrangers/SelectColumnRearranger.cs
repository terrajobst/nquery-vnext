using System.Linq;

using NQuery.Syntax;

namespace NQuery.Authoring.Rearrangement.Rearrangers
{
    internal sealed class SelectColumnRearranger : SeparatedSyntaxListRearranger<SelectColumnSyntax>
    {
        protected override string Description
        {
            get { return "Move SELECT Column"; }
        }

        protected override bool IsHorizontal
        {
            get { return true; }
        }

        protected override SeparatedSyntaxList<SelectColumnSyntax> GetSyntaxList(SelectColumnSyntax node)
        {
            var selectClause = node.Ancestors().OfType<SelectClauseSyntax>().First();
            return selectClause.Columns;
        }
    }
}