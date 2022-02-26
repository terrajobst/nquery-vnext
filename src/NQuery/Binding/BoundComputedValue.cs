namespace NQuery.Binding
{
    internal sealed class BoundComputedValue
    {
        public BoundComputedValue(BoundExpression expression, ValueSlot valueSlot)
        {
            Expression = expression;
            ValueSlot = valueSlot;
        }

        public BoundExpression Expression { get; }

        public ValueSlot ValueSlot { get; }

        public BoundComputedValue Update(BoundExpression expression, ValueSlot valueSlot)
        {
            if (expression == Expression && valueSlot == ValueSlot)
                return this;

            return new BoundComputedValue(expression, valueSlot);
        }
    }
}