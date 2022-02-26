namespace NQuery.Authoring.Outlining
{
    public interface IOutliner
    {
        IEnumerable<OutliningRegionSpan> FindRegions(SyntaxNodeOrToken nodeOrToken);
    }
}