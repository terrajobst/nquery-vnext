using System;

namespace NQuery.Binding
{
    internal sealed class QueryBinder : Binder
    {
        private readonly BoundQueryState _queryState;

        public QueryBinder(SharedBinderState sharedBinderState, Binder parent)
            : base(sharedBinderState, parent)
        {
            _queryState = new BoundQueryState(parent.QueryState);
        }

        public override BoundQueryState QueryState
        {
            get { return _queryState; }
        }

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