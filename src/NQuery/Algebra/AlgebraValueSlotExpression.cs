using System;

using NQuery.Binding;

namespace NQuery.Algebra
{
    internal sealed class AlgebraValueSlotExpression : AlgebraExpression
    {
        private readonly ValueSlot _valueSlot;

        public AlgebraValueSlotExpression(ValueSlot valueSlot)
        {
            _valueSlot = valueSlot;
        }

        public override AlgebraKind Kind
        {
            get { return AlgebraKind.ValueSlotExpression; }
        }

        public ValueSlot ValueSlot
        {
            get { return _valueSlot; }
        }
    }
}