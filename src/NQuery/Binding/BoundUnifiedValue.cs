using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NQuery.Binding
{
    internal sealed class BoundUnifiedValue
    {
        private readonly ValueSlot _valueSlot;
        private readonly ImmutableArray<ValueSlot> _inputValueSlots;

        public BoundUnifiedValue(ValueSlot valueSlot, IEnumerable<ValueSlot> inputValueSlots)
        {
            _valueSlot = valueSlot;
            _inputValueSlots = inputValueSlots.ToImmutableArray();
        }

        public ValueSlot ValueSlot
        {
            get { return _valueSlot; }
        }

        public ImmutableArray<ValueSlot> InputValueSlots
        {
            get { return _inputValueSlots; }
        }

        public BoundUnifiedValue Update(ValueSlot valueSlot, IEnumerable<ValueSlot> inputValueSlots)
        {
            var newInputValueSlots = inputValueSlots.ToImmutableArray();

            if (valueSlot == _valueSlot && newInputValueSlots == _inputValueSlots)
                return this;

            return new BoundUnifiedValue(valueSlot, newInputValueSlots);
        }
    }
}