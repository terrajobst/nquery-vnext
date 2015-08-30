using System;
using System.Collections.Generic;

using NQuery.Syntax;

namespace NQuery.Authoring.Outlining.Outliners
{
    public sealed class SelectQueryOutliner : SyntaxNodeOutliner<SelectQuerySyntax>
    {
        protected override IEnumerable<OutliningRegionSpan> FindRegions(SelectQuerySyntax node)
        {
            var parentIsOrderedQuery = node.Parent is OrderedQuerySyntax;
            if (!parentIsOrderedQuery)
                yield return new OutliningRegionSpan(node.Span, "SELECT");
        }
    }
}