using System;

using NQuery.Binding;

namespace NQuery.Algebra
{
    internal struct AlgebraComputedValue
    {
        private readonly AlgebraExpression _expression;
        private readonly ValueSlot _value;

        public AlgebraComputedValue(AlgebraExpression expression, ValueSlot value)
        {
            _expression = expression;
            _value = value;
        }

        public AlgebraExpression Expression
        {
            get { return _expression; }
        }

        public ValueSlot Value
        {
            get { return _value; }
        }
    }
}