using System.Collections.Immutable;

using NQuery.Binding;

namespace NQuery.Optimization
{
    internal sealed class OuterJoinRemover : BoundTreeRewriter
    {
        private readonly Stack<HashSet<ValueSlot>> _nullRejectedRowBufferEntries = new Stack<HashSet<ValueSlot>>();

        public OuterJoinRemover()
        {
            _nullRejectedRowBufferEntries.Push(new HashSet<ValueSlot>());
        }

        private void AddNullRejectedTable(ValueSlot valueSlot)
        {
            _nullRejectedRowBufferEntries.Peek().Add(valueSlot);
        }

        private bool IsAnyNullRejected(ImmutableArray<ValueSlot> valueSlots)
        {
            return valueSlots.Any(_nullRejectedRowBufferEntries.Peek().Contains);
        }

        private static BoundRelation WrapWithFilter(BoundRelation input, BoundExpression predicate)
        {
            return predicate is null
                ? input
                : new BoundFilterRelation(input, predicate);
        }

        protected override BoundRelation RewriteFilterRelation(BoundFilterRelation node)
        {
            // Check for null rejecting conditions.

            var dependencyFinder = new ValueSlotDependencyFinder();

            foreach (var conjunction in Expression.SplitConjunctions(node.Condition))
            {
                dependencyFinder.ValueSlots.Clear();
                dependencyFinder.VisitExpression(conjunction);

                var slots = dependencyFinder.ValueSlots;
                var nullRejectedSlots = slots.Where(v => NullRejection.IsRejectingNull(conjunction, v));

                foreach (var valueSlot in nullRejectedSlots)
                    AddNullRejectedTable(valueSlot);
            }

            return base.RewriteFilterRelation(node);
        }

        protected override BoundRelation RewriteJoinRelation(BoundJoinRelation node)
        {
            // Visit children

            _nullRejectedRowBufferEntries.Push(new HashSet<ValueSlot>());

            node = (BoundJoinRelation) base.RewriteJoinRelation(node);

            // Get declared tables of left and right

            var leftDefinedValues = node.Left.GetDefinedValues().ToImmutableArray();
            var rightDefinedValues = node.Right.GetDefinedValues().ToImmutableArray();

            // Analyze AND-parts of Condition

            if (node.JoinType != BoundJoinType.FullOuter)
            {
                var dependencyFinder = new ValueSlotDependencyFinder();

                foreach (var conjunction in Expression.SplitConjunctions(node.Condition))
                {
                    // Check if we can derive from this conjunction that a table it depends on
                    // is null-rejected.

                    dependencyFinder.ValueSlots.Clear();
                    dependencyFinder.VisitExpression(conjunction);

                    var slots = dependencyFinder.ValueSlots;
                    var nullRejectedSlots = slots.Where(v => NullRejection.IsRejectingNull(conjunction, v));

                    foreach (var valueSlot in nullRejectedSlots)
                    {
                        if (node.JoinType != BoundJoinType.LeftOuter && leftDefinedValues.Contains(valueSlot))
                        {
                            AddNullRejectedTable(valueSlot);
                        }
                        else if (node.JoinType != BoundJoinType.RightOuter && rightDefinedValues.Contains(valueSlot))
                        {
                            AddNullRejectedTable(valueSlot);
                        }
                    }
                }
            }

            // Replace outer joins by left-/right-/inner joins

            if (node.JoinType == BoundJoinType.RightOuter ||
                node.JoinType == BoundJoinType.FullOuter)
            {
                if (IsAnyNullRejected(leftDefinedValues))
                {
                    var newType = node.JoinType == BoundJoinType.RightOuter
                        ? BoundJoinType.Inner
                        : BoundJoinType.LeftOuter;

                    node = node.WithJoinType(newType);
                }
            }

            if (node.JoinType == BoundJoinType.LeftOuter ||
                node.JoinType == BoundJoinType.FullOuter)
            {
                if (IsAnyNullRejected(rightDefinedValues))
                {
                    var newType = node.JoinType == BoundJoinType.LeftOuter
                        ? BoundJoinType.Inner
                        : BoundJoinType.RightOuter;

                    node = node.WithJoinType(newType);
                }
            }

            // As we're done with the main logic it's time to merge the stack

            var childrenNullRejectedRowBufferEntries = _nullRejectedRowBufferEntries.Pop();
            _nullRejectedRowBufferEntries.Peek().UnionWith(childrenNullRejectedRowBufferEntries);

            // After converting an outer join to an inner one we can
            // sometimes eliminate the whole join.

            if (node.JoinType == BoundJoinType.Inner)
            {
                if (node.Left is BoundConstantRelation && !node.Left.GetDefinedValues().Any())
                    return RewriteRelation(WrapWithFilter(node.Right, node.Condition));

                if (node.Right is BoundConstantRelation && !node.Right.GetDefinedValues().Any())
                    return RewriteRelation(WrapWithFilter(node.Left, node.Condition));
            }

            return node;
        }
    }
}