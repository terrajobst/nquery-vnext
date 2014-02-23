using System;
using System.Collections;

namespace NQuery.Binding
{
    internal struct BoundSortedValue
    {
        private readonly ValueSlot _valueSlot;
        private readonly IComparer _comparer;

        public BoundSortedValue(ValueSlot valueSlot, IComparer comparer)
        {
            _valueSlot = valueSlot;
            _comparer = comparer;
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