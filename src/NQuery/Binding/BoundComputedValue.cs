using System;

namespace NQuery.Binding
{
    internal sealed class BoundComputedValue
    {
        private readonly BoundExpression _expression;
        private readonly ValueSlot _valueSlot;

        public BoundComputedValue(BoundExpression expression, ValueSlot valueSlot)
        {
            _expression = expression;
            _valueSlot = valueSlot;
        }

        public BoundExpression Expression
        {
            get { return _expression; }
        }

        public ValueSlot ValueSlot
        {
            get { return _valueSlot; }
        }

        public BoundComputedValue Update(BoundExpression expression, ValueSlot valueSlot)
        {
            if (expression == _expression && valueSlot == _valueSlot)
                return this;

            return new BoundComputedValue(expression, valueSlot);
        }
    }
}