using System;
using System.Collections;

namespace NQuery.Binding
{
    internal sealed class BoundSortedValue
    {
        public BoundSortedValue(ValueSlot valueSlot, IComparer comparer)
        {
            ValueSlot = valueSlot;
            Comparer = comparer;
        }

        public ValueSlot ValueSlot { get; }

        public IComparer Comparer { get; }

        public BoundSortedValue Update(ValueSlot valueSlot, IComparer comparer)
        {
            if (valueSlot == ValueSlot && comparer == Comparer)
                return this;

            return new BoundSortedValue(valueSlot, comparer);
        }
    }
}