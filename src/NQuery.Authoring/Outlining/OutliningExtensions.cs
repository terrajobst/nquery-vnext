using System.Collections.Immutable;

using NQuery.Authoring.Outlining.Outliners;
using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.Outlining;

public static class OutliningExtensions
{
    public static ImmutableArray<IOutliner> StandardOutliners { get; } =
    [
        new SelectQueryOutliner(),
        new OrderedQueryOutliner(),
        new MultiLineCommentOutliner(),
        new SingleLineCommentOutliner()
    ];

    extension(SyntaxNode root)
    {
        public IReadOnlyList<OutliningRegionSpan> FindRegions()
        {
            ThrowIfNull(root);

            return root.FindRegions(root.FullSpan);
        }

        public IReadOnlyList<OutliningRegionSpan> FindRegions(IEnumerable<IOutliner> outliners)
        {
            ThrowIfNull(root);

            return root.FindRegions(root.FullSpan, outliners);
        }

        public IReadOnlyList<OutliningRegionSpan> FindRegions(TextSpan span)
        {
            return root.FindRegions(span, StandardOutliners);
        }

        public IReadOnlyList<OutliningRegionSpan> FindRegions(TextSpan span, IEnumerable<IOutliner> outliners)
        {
            ThrowIfNull(root);
            ThrowIfNull(outliners);

            var result = new List<OutliningRegionSpan>();
            var worker = new OutliningWorker(root.SyntaxTree.Text, result, span, outliners);
            worker.Visit(root);
            return result;
        }
    }
}
