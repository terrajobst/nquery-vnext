using System;

namespace NQuery.Binding
{
    internal struct BoundOrderBySelector
    {
        private readonly ValueSlot _valueSlot;
        private readonly BoundComputedValueWithSyntax? _computedValue;

        public BoundOrderBySelector(ValueSlot valueSlot, BoundComputedValueWithSyntax? computedValue)
        {
            _valueSlot = valueSlot;
            _computedValue = computedValue;
        }

        public ValueSlot ValueSlot
        {
            get { return _valueSlot; }
        }

        public BoundComputedValueWithSyntax? ComputedValue
        {
            get { return _computedValue; }
        }
    }
}