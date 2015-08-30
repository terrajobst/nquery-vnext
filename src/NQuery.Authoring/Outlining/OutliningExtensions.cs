using System;
using System.Collections.Generic;

using NQuery.Authoring.Outlining.Outliners;
using NQuery.Text;

namespace NQuery.Authoring.Outlining
{
    public static class OutliningExtensions
    {
        public static IEnumerable<IOutliner> GetStandardOutliners()
        {
            return new IOutliner[]
            {
                new SelectQueryOutliner(),
                new OrderedQueryOutliner(),
                new MultiLineCommentOutliner(),
                new SingleLineCommentOutliner()
            };
        }

        public static IReadOnlyList<OutliningRegionSpan> FindRegions(this SyntaxNode root)
        {
            return root.FindRegions(root.FullSpan);
        }

        public static IReadOnlyList<OutliningRegionSpan> FindRegions(this SyntaxNode root, IEnumerable<IOutliner> outliners)
        {
            return root.FindRegions(root.FullSpan, outliners);
        }

        public static IReadOnlyList<OutliningRegionSpan> FindRegions(this SyntaxNode root, TextSpan span)
        {
            var outliners = GetStandardOutliners();
            return root.FindRegions(span, outliners);
        }

        public static IReadOnlyList<OutliningRegionSpan> FindRegions(this SyntaxNode root, TextSpan span, IEnumerable<IOutliner> outliners)
        {
            var result = new List<OutliningRegionSpan>();
            var worker = new OutliningWorker(root.SyntaxTree.Text, result, span, outliners);
            worker.Visit(root);
            return result;
        }
    }
}