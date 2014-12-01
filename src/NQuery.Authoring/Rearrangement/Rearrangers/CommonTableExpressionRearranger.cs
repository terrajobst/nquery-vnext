using System;
using System.Linq;

using NQuery.Syntax;

namespace NQuery.Authoring.Rearrangement.Rearrangers
{
    internal sealed class CommonTableExpressionRearranger : SeparatedSyntaxListRearranger<CommonTableExpressionSyntax>
    {
        protected override string Description
        {
            get { return "Move commont table expression"; }
        }

        protected override bool IsHorizontal
        {
            get { return false; }
        }

        protected override SeparatedSyntaxList<CommonTableExpressionSyntax> GetSyntaxList(CommonTableExpressionSyntax node)
        {
            var selectClause = node.Ancestors().OfType<CommonTableExpressionQuerySyntax>().First();
            return selectClause.CommonTableExpressions;
        }
    }
}