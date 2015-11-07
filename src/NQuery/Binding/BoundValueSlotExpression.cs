using System;

namespace NQuery.Binding
{
    internal sealed class BoundValueSlotExpression : BoundExpression
    {
        public BoundValueSlotExpression(ValueSlot valueSlot)
        {
            ValueSlot = valueSlot;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.ValueSlotExpression; }
        }

        public override Type Type
        {
            get { return ValueSlot.Type; }
        }

        public ValueSlot ValueSlot { get; }

        public BoundValueSlotExpression Update(ValueSlot valueSlot)
        {
            if (valueSlot == ValueSlot)
                return this;

            return new BoundValueSlotExpression(valueSlot);
        }

        public override string ToString()
        {
            return ValueSlot.Name;
        }
    }
}