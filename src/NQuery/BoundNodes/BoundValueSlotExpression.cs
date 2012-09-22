using System;

using NQuery.Algebra;
using NQuery.BoundNodes;

namespace NQuery.BoundNodes
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
            get { return BoundNodeKind.BoundValueSlot; }
        }

        public override Type Type
        {
            get { return _valueSlot.Type; }
        }

        public override string ToString()
        {
            return string.Format("[{0}]", _valueSlot.DisplayName);
        }
    }
}