namespace NQuery.Binding
{
    internal sealed class BoundFromClause
    {
        public BoundFromClause(BoundTableReference root)
        {
            Root = root;
        }

        public BoundTableReference Root { get; }
    }
}
