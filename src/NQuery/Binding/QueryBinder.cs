using System;

namespace NQuery.Binding
{
    internal sealed class QueryBinder : Binder
    {
        private readonly QueryState _queryState;

        public QueryBinder(SharedBinderState sharedBinderState, Binder parent)
            : base(sharedBinderState, parent)
        {
            _queryState = new QueryState(parent.QueryState);
        }

        public override QueryState QueryState
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