using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using NQuery.Symbols;

namespace NQuery.Binding
{
    internal sealed class BoundSelectQuery : BoundQuery
    {
        private readonly Binder _fromAwareBinder;
        private readonly ReadOnlyCollection<QueryColumnInstanceSymbol> _outputColumns;
        private readonly int? _top;
        private readonly bool _withTies;
        private readonly BoundTableReference _fromClause;
        private readonly BoundExpression _whereClause;
        private readonly ReadOnlyCollection<Tuple<BoundAggregateExpression, ValueSlot>> _aggregates;
        private readonly ReadOnlyCollection<Tuple<BoundExpression, ValueSlot>> _groups;
        private readonly BoundExpression _havingClause;
        private readonly ReadOnlyCollection<Tuple<BoundExpression, ValueSlot>> _projections;
        private readonly BoundOrderByClause _orderByClause;

        public BoundSelectQuery(Binder fromAwareBinder, int? top, bool withTies, BoundTableReference fromClause, BoundExpression whereClause, IList<Tuple<BoundAggregateExpression, ValueSlot>> aggregates, IList<Tuple<BoundExpression, ValueSlot>> groups, BoundExpression havingClause, IList<Tuple<BoundExpression, ValueSlot>> projections, BoundOrderByClause orderByClause, IList<QueryColumnInstanceSymbol> outputColumns)
        {
            _fromAwareBinder = fromAwareBinder;
            _outputColumns = new ReadOnlyCollection<QueryColumnInstanceSymbol>(outputColumns);
            _top = top;
            _withTies = withTies;
            _fromClause = fromClause;
            _whereClause = whereClause;
            _havingClause = havingClause;
            _orderByClause = orderByClause;
            _groups = new ReadOnlyCollection<Tuple<BoundExpression, ValueSlot>>(groups);
            _projections = new ReadOnlyCollection<Tuple<BoundExpression, ValueSlot>>(projections);
            _aggregates = new ReadOnlyCollection<Tuple<BoundAggregateExpression, ValueSlot>>(aggregates);
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.SelectQuery; }
        }

        public override ReadOnlyCollection<QueryColumnInstanceSymbol> OutputColumns
        {
            get { return _outputColumns; }
        }

        public Binder FromAwareBinder
        {
            get { return _fromAwareBinder; }
        }

        public int? Top
        {
            get { return _top; }
        }

        public bool WithTies
        {
            get { return _withTies; }
        }

        public BoundTableReference FromClause
        {
            get { return _fromClause; }
        }

        public BoundExpression WhereClause
        {
            get { return _whereClause; }
        }

        public ReadOnlyCollection<Tuple<BoundAggregateExpression, ValueSlot>> Aggregates
        {
            get { return _aggregates; }
        }

        public ReadOnlyCollection<Tuple<BoundExpression, ValueSlot>> Groups
        {
            get { return _groups; }
        }

        public BoundExpression HavingClause
        {
            get { return _havingClause; }
        }

        public ReadOnlyCollection<Tuple<BoundExpression, ValueSlot>> Projections
        {
            get { return _projections; }
        }

        public BoundOrderByClause OrderByClause
        {
            get { return _orderByClause; }
        }
    }
}