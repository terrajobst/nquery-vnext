using NQuery.CodeAnalysis;

namespace NQuery.Authoring.Outlining;

public abstract class SyntaxTokenOutliner : IOutliner
{
    public IEnumerable<OutliningRegionSpan> FindRegions(Document document, CancellationToken cancellationToken)
    {
        ThrowIfNull(document);

        var root = document.GetSyntaxTree(cancellationToken).Root;
        return root.DescendantTokens().SelectMany(FindRegions);
    }

    protected abstract IEnumerable<OutliningRegionSpan> FindRegions(SyntaxToken token);
}
