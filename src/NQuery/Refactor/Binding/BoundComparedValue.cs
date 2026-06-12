using System.Collections;

using NQuery.Binding;

namespace NQuery.Refactor.Binding
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

    }
}