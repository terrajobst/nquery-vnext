namespace NQuery.Binding
{
    internal sealed class QueryBinder : Binder
    {
        public QueryBinder(SharedBinderState sharedBinderState, Binder parent)
            : base(sharedBinderState, parent)
        {
            QueryState = new BoundQueryState(parent.QueryState);
        }

        public override BoundQueryState QueryState { get; }

        protected override bool InWhereClause
        {
            get { return false; }
        }

        protected override bool InOnClause
        {
            get { return false; }
        }

        protected override bool InGroupByClause
        {
            get { return false; }
        }

        protected override bool InAggregateArgument
        {
            get { return false; }
        }
    }
}