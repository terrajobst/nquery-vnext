using System;
using System.Collections;

using NQuery.Symbols;

namespace NQuery.Binding
{
    internal sealed class BoundOrderByColumn : BoundNode
    {
        private readonly QueryColumnInstanceSymbol _queryColumn;
        private readonly ValueSlot _valueSlot;
        private readonly IComparer _comparer;

        public BoundOrderByColumn(QueryColumnInstanceSymbol queryColumn, ValueSlot valueSlot, IComparer comparer)
        {
            _queryColumn = queryColumn;
            _valueSlot = valueSlot;
            _comparer = comparer;
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

        public IComparer Comparer
        {
            get { return _comparer; }
        }
    }
}