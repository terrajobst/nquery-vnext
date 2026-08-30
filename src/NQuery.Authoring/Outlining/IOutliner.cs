namespace NQuery.Authoring.Outlining;

public interface IOutliner
{
    IEnumerable<OutliningRegionSpan> FindRegions(Document document, CancellationToken cancellationToken);
}
