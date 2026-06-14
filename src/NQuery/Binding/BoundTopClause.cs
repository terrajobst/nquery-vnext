namespace NQuery.Binding
{
    internal sealed class BoundTopClause
    {
        public BoundTopClause(int limit, bool withTies)
        {
            Limit = limit;
            WithTies = withTies;
        }

        public int Limit { get; }

        public bool WithTies { get; }
    }
}
