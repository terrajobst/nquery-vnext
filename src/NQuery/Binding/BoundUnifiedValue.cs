using System.Collections.Immutable;

namespace NQuery.Binding
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

        public BoundUnifiedValue Update(ValueSlot valueSlot, IEnumerable<ValueSlot> inputValueSlots)
        {
            var newInputValueSlots = inputValueSlots.ToImmutableArray();

            if (valueSlot == ValueSlot && newInputValueSlots == InputValueSlots)
                return this;

            return new BoundUnifiedValue(valueSlot, newInputValueSlots);
        }
    }
}