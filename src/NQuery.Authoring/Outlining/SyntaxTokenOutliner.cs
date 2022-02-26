namespace NQuery.Authoring.Outlining
{
    public abstract class SyntaxTokenOutliner : IOutliner
    {
        public IEnumerable<OutliningRegionSpan> FindRegions(SyntaxNodeOrToken nodeOrToken)
        {
            return !nodeOrToken.IsToken
                ? Enumerable.Empty<OutliningRegionSpan>()
                : FindRegions(nodeOrToken.AsToken());
        }

        protected abstract IEnumerable<OutliningRegionSpan> FindRegions(SyntaxToken token);
    }
}