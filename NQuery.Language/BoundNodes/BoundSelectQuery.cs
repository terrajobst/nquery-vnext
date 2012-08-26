using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NQuery.Language.BoundNodes
{
    internal sealed class BoundSelectQuery : BoundQuery
    {
        private readonly ReadOnlyCollection<BoundSelectColumn> _selectColumns;
        private readonly BoundTableReference _fromClause;
        private readonly BoundExpression _whereClause;
        private readonly BoundExpression _havingClause;

        public BoundSelectQuery(IList<BoundSelectColumn> selectColumns, BoundTableReference fromClause, BoundExpression whereClause, BoundExpression havingClause)
        {
            _selectColumns = new ReadOnlyCollection<BoundSelectColumn>(selectColumns);
            _fromClause = fromClause;
            _whereClause = whereClause;
            _havingClause = havingClause;
        }

        public override ReadOnlyCollection<BoundSelectColumn> SelectColumns
        {
            get { return _selectColumns; }
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.SelectQuery; }
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