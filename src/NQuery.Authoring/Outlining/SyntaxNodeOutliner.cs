using NQuery.CodeAnalysis;

namespace NQuery.Authoring.Outlining;

public abstract class SyntaxNodeOutliner<T> : IOutliner
    where T : SyntaxNode
{
    public IEnumerable<OutliningRegionSpan> FindRegions(Document document, CancellationToken cancellationToken)
    {
        ThrowIfNull(document);

        var root = document.GetSyntaxTree(cancellationToken).Root;
        return root.DescendantNodesAndSelf().OfType<T>().SelectMany(FindRegions);
    }

    protected abstract IEnumerable<OutliningRegionSpan> FindRegions(T node);
}
