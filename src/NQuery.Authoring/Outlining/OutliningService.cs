using System.Collections.Immutable;

using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.Outlining;

public sealed class OutliningService
{
    private readonly ImmutableArray<IOutliner> _outliners;

    public OutliningService(ImmutableArray<IOutliner> outliners)
    {
        _outliners = outliners;
    }

    public ImmutableArray<OutliningRegionSpan> FindRegions(Document document, CancellationToken cancellationToken = default)
    {
        ThrowIfNull(document);

        var root = document.GetSyntaxTree(cancellationToken).Root;
        return FindRegions(document, root.FullSpan, cancellationToken);
    }

    // Single-line regions are dropped here rather than in each outliner: whether a region is worth
    // collapsing is a property of the feature, not of the construct that produced it.
    public ImmutableArray<OutliningRegionSpan> FindRegions(Document document, TextSpan span, CancellationToken cancellationToken = default)
    {
        ThrowIfNull(document);

        var text = document.Text;

        return [.. from outliner in _outliners
                   from region in outliner.FindRegions(document, cancellationToken)
                   where region.Span.IntersectsWith(span) && SpansMultipleLines(text, region.Span)
                   select region];
    }

    private static bool SpansMultipleLines(SourceText text, TextSpan span)
    {
        var start = text.GetLineFromPosition(span.Start);
        var end = text.GetLineFromPosition(span.End);
        return start.LineNumber != end.LineNumber;
    }
}
