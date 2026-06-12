using System.Collections.Immutable;

using NQuery.Binding;

namespace NQuery.Refactor.Binding
{
    internal sealed class BoundUnifiedValue
    {
        public BoundUnifiedValue(ValueSlot valueSlot, IEnumerable<ValueSlot> inputValueSlots)
        {
            ValueSlot = valueSlot;
            InputValueSlots = inputValueSlots.ToImmutableArray();
        }

        public ValueSlot ValueSlot { get; }

        public ImmutableArray<ValueSlot> InputValueSlots { get; }

    }
}