using System;

using NQuery.Symbols;

namespace NQuery.Binding
{
    internal sealed class BoundOrderByColumn : BoundNode
    {
        private readonly QueryColumnInstanceSymbol _queryColumn;
        private readonly ValueSlot _valueSlot;
        private readonly bool _isAscending;

        public BoundOrderByColumn(QueryColumnInstanceSymbol queryColumn, ValueSlot valueSlot, bool isAscending)
        {
            _queryColumn = queryColumn;
            _valueSlot = valueSlot;
            _isAscending = isAscending;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.OrderByColumn; }
        }

        public QueryColumnInstanceSymbol QueryColumn
        {
            get { return _queryColumn; }
        }

        public ValueSlot ValueSlot
        {
            get { return _valueSlot; }
        }

        public bool IsAscending
        {
            get { return _isAscending; }
        }
    }
}