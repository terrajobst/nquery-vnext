using System.Collections;

namespace NQuery.Binding
{
    internal sealed class BoundComparedValue
    {
        public BoundComparedValue(ValueSlot valueSlot, IComparer comparer)
        {
            ValueSlot = valueSlot;
            Comparer = comparer;
        }

        public ValueSlot ValueSlot { get; }

        public IComparer Comparer { get; }

        public BoundComparedValue Update(ValueSlot valueSlot, IComparer comparer)
        {
            if (valueSlot == ValueSlot && comparer == Comparer)
                return this;

            return new BoundComparedValue(valueSlot, comparer);
        }
    }
}