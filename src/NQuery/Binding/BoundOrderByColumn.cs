using System;

using NQuery.Symbols;

namespace NQuery.Binding
{
    internal sealed class BoundOrderByColumn : BoundNode
    {
        private readonly QueryColumnInstanceSymbol _queryColumn;
        private readonly BoundSortedValue _sortedValue;

        public BoundOrderByColumn(QueryColumnInstanceSymbol queryColumn, BoundSortedValue sortedValue)
        {
            _queryColumn = queryColumn;
            _sortedValue = sortedValue;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.OrderByColumn; }
        }

        public QueryColumnInstanceSymbol QueryColumn
        {
            get { return _queryColumn; }
        }

        public BoundSortedValue SortedValue
        {
            get { return _sortedValue; }
        }
    }
}