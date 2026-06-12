using NQuery.Binding;

namespace NQuery.Refactor.Binding
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

    }
}