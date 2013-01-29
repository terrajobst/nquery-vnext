using System;
using System.Collections.Generic;

using NQuery.BoundNodes;

namespace NQuery.Binding
{
    internal sealed class QueryBinder : Binder
    {
        private readonly QueryState _queryState;

        public QueryBinder(Binder parent, Dictionary<SyntaxNode, BoundNode> boundNodeFromSynatxNode, Dictionary<BoundNode, Binder> binderFromBoundNode, List<Diagnostic> diagnostics, ValueSlotFactory valueSlotFactory)
            : base(parent, boundNodeFromSynatxNode, binderFromBoundNode, diagnostics, valueSlotFactory)
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