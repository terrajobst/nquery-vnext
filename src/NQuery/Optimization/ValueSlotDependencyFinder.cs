using System;
using System.Collections.Generic;

using NQuery.Binding;

namespace NQuery.Optimization
{
    internal sealed class ValueSlotDependencyFinder : BoundTreeWalker
    {
        private readonly HashSet<ValueSlot> _valueSlots;

        public ValueSlotDependencyFinder()
        {
            _valueSlots = new HashSet<ValueSlot>();
        }

        public ValueSlotDependencyFinder(HashSet<ValueSlot> valueSlots)
        {
            _valueSlots = valueSlots;
        }

        public HashSet<ValueSlot> ValueSlots
        {
            get { return _valueSlots; }
        }

        protected override void VisitValueSlotExpression(BoundValueSlotExpression node)
        {
            _valueSlots.Add(node.ValueSlot);
            base.VisitValueSlotExpression(node);
        }
    }
}