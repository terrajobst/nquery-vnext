using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.Binding;

namespace NQuery.Optimization
{
    internal static class OuterReferenceFinder
    {
        public static IEnumerable<ValueSlot> GetOuterReferences(BoundJoinRelation node)
        {
            var valueSlotDependencyFinder = new ValueSlotDependencyFinder();
            valueSlotDependencyFinder.VisitRelation(node.Right);
            var usedValueSlots = valueSlotDependencyFinder.ValueSlots;

            return node.Left.GetDefinedValues().Where(usedValueSlots.Contains);
        }
    }
}