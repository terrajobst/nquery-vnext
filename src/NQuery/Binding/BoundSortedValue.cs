using System;
using System.Collections;

namespace NQuery.Binding
{
    internal sealed class BoundSortedValue
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

        public BoundSortedValue Update(ValueSlot valueSlot, IComparer comparer)
        {
            if (valueSlot == _valueSlot && comparer == _comparer)
                return this;

            return new BoundSortedValue(valueSlot, comparer);
        }
    }
}