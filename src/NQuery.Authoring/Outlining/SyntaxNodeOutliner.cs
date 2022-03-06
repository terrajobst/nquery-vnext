namespace NQuery.Authoring.Outlining
{
    public abstract class SyntaxNodeOutliner<T> : IOutliner
        where T : SyntaxNode
    {

        public IEnumerable<OutliningRegionSpan> FindRegions(SyntaxNodeOrToken nodeOrToken)
        {
            var node = nodeOrToken.IsNode ? nodeOrToken.AsNode() : null;
            return node is not T typedNode
                ? Enumerable.Empty<OutliningRegionSpan>()
                : FindRegions(typedNode);
        }

        protected abstract IEnumerable<OutliningRegionSpan> FindRegions(T node);
    }
}