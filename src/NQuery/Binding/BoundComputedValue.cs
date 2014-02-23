using System;

namespace NQuery.Binding
{
    internal struct BoundComputedValue
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
    }
}