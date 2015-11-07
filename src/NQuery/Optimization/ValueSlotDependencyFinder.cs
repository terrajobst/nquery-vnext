using System;
using System.Collections.Generic;

using NQuery.Binding;

namespace NQuery.Optimization
{
    internal sealed class ValueSlotDependencyFinder : BoundTreeWalker
    {
        public ValueSlotDependencyFinder()
        {
            ValueSlots = new HashSet<ValueSlot>();
        }

        public ValueSlotDependencyFinder(HashSet<ValueSlot> valueSlots)
        {
            ValueSlots = valueSlots;
        }

        public HashSet<ValueSlot> ValueSlots { get; }

        protected override void VisitValueSlotExpression(BoundValueSlotExpression node)
        {
            ValueSlots.Add(node.ValueSlot);
            base.VisitValueSlotExpression(node);
        }
    }
}