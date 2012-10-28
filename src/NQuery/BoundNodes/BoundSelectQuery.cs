using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NQuery.BoundNodes
{
    internal sealed class BoundSelectQuery : BoundQuery
    {
        private readonly ReadOnlyCollection<BoundSelectColumn> _selectColumns;
        private readonly int? _top;
        private readonly bool _withTies;
        private readonly BoundTableReference _fromClause;
        private readonly BoundExpression _whereClause;
        private readonly BoundExpression _havingClause;

        public BoundSelectQuery(IList<BoundSelectColumn> selectColumns, int? top, bool withTies, BoundTableReference fromClause, BoundExpression whereClause, BoundExpression havingClause)
        {
            _selectColumns = new ReadOnlyCollection<BoundSelectColumn>(selectColumns);
            _top = top;
            _withTies = withTies;
            _fromClause = fromClause;
            _whereClause = whereClause;
            _havingClause = havingClause;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.SelectQuery; }
        }

        public override ReadOnlyCollection<BoundSelectColumn> SelectColumns
        {
            get { return _selectColumns; }
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

        public BoundExpression HavingClause
        {
            get { return _havingClause; }
        }
    }
}