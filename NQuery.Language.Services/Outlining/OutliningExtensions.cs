using System;
using System.Collections.Generic;

namespace NQuery.Language.Services.Outlining
{
    public static class OutliningExtensions
    {
        public static IReadOnlyList<OutliningRegionSpan> FindRegions(this SyntaxNode root)
        {
            return root.FindRegions(root.FullSpan);
        }

        public static IReadOnlyList<OutliningRegionSpan> FindRegions(this SyntaxNode root, TextSpan span)
        {
            var result = new List<OutliningRegionSpan>();
            var worker = new OutliningWorker(root.SyntaxTree.TextBuffer, result, span);
            worker.Visit(root);
            return result;
        }        
    }
}