using System.Collections.Immutable;

using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.Outlining;

public sealed class OutliningService
{
    private readonly ImmutableArray<IOutliner> _outliners;

    internal OutliningService(ImmutableArray<IOutliner> outliners)
    {
        _outliners = outliners;
    }

    public ImmutableArray<OutliningRegionSpan> FindRegions(Document document, CancellationToken cancellationToken = default)
    {
        ThrowIfNull(document);

        var root = document.GetSyntaxTree(cancellationToken).Root;
        return FindRegions(document, root.FullSpan, cancellationToken);
    }

    public ImmutableArray<OutliningRegionSpan> FindRegions(Document document, TextSpan span, CancellationToken cancellationToken = default)
    {
        ThrowIfNull(document);

        var root = document.GetSyntaxTree(cancellationToken).Root;

        var result = new List<OutliningRegionSpan>();
        var worker = new OutliningWorker(root.SyntaxTree.Text, result, span, _outliners);
        worker.Visit(root);
        return [.. result];
    }
}
