using System;
using System.Collections.Generic;

namespace NQuery.Authoring.Outlining
{
    public interface IOutliner
    {
        IEnumerable<OutliningRegionSpan> FindRegions(SyntaxNodeOrToken nodeOrToken);
    }
}