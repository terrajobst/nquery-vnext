namespace NQuery.Binding
{
    internal struct BoundOrderBySelector
    {
        public BoundOrderBySelector(ValueSlot valueSlot, BoundComputedValueWithSyntax? computedValue)
        {
            ValueSlot = valueSlot;
            ComputedValue = computedValue;
        }

        public ValueSlot ValueSlot { get; }

        public BoundComputedValueWithSyntax? ComputedValue { get; }
    }
}