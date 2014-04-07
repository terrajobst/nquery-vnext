using System;

namespace NQuery.Binding
{
    internal sealed class BoundValueSlotExpression : BoundExpression
    {
        private readonly ValueSlot _valueSlot;

        public BoundValueSlotExpression(ValueSlot valueSlot)
        {
            _valueSlot = valueSlot;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.ValueSlotExpression; }
        }

        public override Type Type
        {
            get { return _valueSlot.Type; }
        }

        public ValueSlot ValueSlot
        {
            get { return _valueSlot; }
        }

        public BoundValueSlotExpression Update(ValueSlot valueSlot)
        {
            if (valueSlot == _valueSlot)
                return this;

            return new BoundValueSlotExpression(valueSlot);
        }

        public override string ToString()
        {
            return _valueSlot.Name;
        }
    }
}