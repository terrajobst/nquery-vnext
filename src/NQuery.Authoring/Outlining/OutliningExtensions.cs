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
        return root.FindRegions(span, StandardOutliners);
    }

    public static IReadOnlyList<OutliningRegionSpan> FindRegions(this SyntaxNode root, TextSpan span, IEnumerable<IOutliner> outliners)
    {
        var result = new List<OutliningRegionSpan>();
        var worker = new OutliningWorker(root.SyntaxTree.Text, result, span, outliners);
        worker.Visit(root);
        return result;
    }
}
