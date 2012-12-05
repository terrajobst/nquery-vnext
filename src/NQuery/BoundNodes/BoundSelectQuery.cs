using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using System.Linq;

using NQuery.Binding;
using NQuery.Symbols;

namespace NQuery.BoundNodes
{
    internal sealed class BoundSelectQuery : BoundQuery
    {
        private readonly ReadOnlyCollection<QueryColumnInstanceSymbol> _outputColumns;
        private readonly int? _top;
        private readonly bool _withTies;
        private readonly ReadOnlyCollection<BoundSelectColumn> _selectColumns;
        private readonly BoundTableReference _fromClause;
        private readonly BoundExpression _whereClause;
        private readonly ReadOnlyCollection<BoundProjectedValue> _localValues;
        private readonly BoundGroupByClause _groupByClause;
        private readonly BoundExpression _havingClause;
        private readonly BoundOrderByClause _orderByClause;

        public BoundSelectQuery(int? top, bool withTies, IList<BoundSelectColumn> selectColumns, BoundTableReference fromClause, BoundExpression whereClause, IList<BoundProjectedValue> localValues, BoundGroupByClause groupByClause, BoundExpression havingClause, BoundOrderByClause orderByClause, IList<QueryColumnInstanceSymbol> outputColumns)
        {
            _outputColumns = new ReadOnlyCollection<QueryColumnInstanceSymbol>(outputColumns);
            _selectColumns = new ReadOnlyCollection<BoundSelectColumn>(selectColumns);
            _top = top;
            _withTies = withTies;
            _fromClause = fromClause;
            _whereClause = whereClause;
            _localValues = new ReadOnlyCollection<BoundProjectedValue>(localValues);
            _groupByClause = groupByClause;
            _havingClause = havingClause;
            _orderByClause = orderByClause;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.SelectQuery; }
        }

        public override ReadOnlyCollection<QueryColumnInstanceSymbol> OutputColumns
        {
            get { return _outputColumns; }
        }

        public int? Top
        {
            get { return _top; }
        }

        public bool WithTies
        {
            get { return _withTies; }
        }

        public ReadOnlyCollection<BoundSelectColumn> SelectColumns
        {
            get { return _selectColumns; }
        }

        public BoundTableReference FromClause
        {
            get { return _fromClause; }
        }

        public BoundExpression WhereClause
        {
            get { return _whereClause; }
        }

        public ReadOnlyCollection<BoundProjectedValue> LocalValues
        {
            get { return _localValues; }
        }

        public BoundGroupByClause GroupByClause
        {
            get { return _groupByClause; }
        }

        public BoundExpression HavingClause
        {
            get { return _havingClause; }
        }

        public BoundOrderByClause OrderByClause
        {
            get { return _orderByClause; }
        }
    }
}