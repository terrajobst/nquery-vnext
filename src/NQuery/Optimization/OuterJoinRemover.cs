#nullable enable

using System.Collections.Frozen;
using System.Collections.Immutable;

using NQuery.Algebra;

namespace NQuery.Optimization
{
    // Null-rejection outer-join simplification. When a predicate above an outer join is
    // guaranteed to reject the rows where the null-supplied side is NULL, those padded
    // rows can never survive, so the join can be tightened:
    //
    //   * LEFT OUTER  -> INNER       when the right side is null-rejected.
    //   * FULL OUTER  -> INNER       when both sides are null-rejected.
    //   * FULL OUTER  -> LEFT OUTER  when only the left side is null-rejected.
    //   * FULL OUTER  -> LEFT OUTER (operands swapped) when only the right is null-rejected
    //     -- that case is a RIGHT OUTER, which the logical layer doesn't model, so it is
    //     swapped to the equivalent left outer (the same normalization the algebrizer uses
    //     for a RIGHT JOIN; safe because operands are referenced by identity and the
    //     enclosing projection fixes column order).
    //
    // Rejection is collected top-down: a filter's conjuncts, and an inner/semi join's own
    // condition conjuncts, contribute the slots they reject. The accumulating set is never
    // cleared as we descend -- a value slot is a unique identity, so a rejection that
    // "leaks" to a sibling subtree can't match a slot defined there. Running this before
    // join ordering lets a tightened inner join join its region and accept pushed-down
    // selections.
    //
    // The pass also folds in one non-null-rejection simplification the legacy
    // OuterJoinRemover carried (it composes naturally once an outer join becomes inner):
    // an inner join against the single-row constant relation (a query with no FROM) is the
    // other side with the join condition as a filter.
    internal sealed class OuterJoinRemover : LogicalOperatorRewriter
    {
        // Slots known to be null-rejected by a predicate above the current node. Instance
        // state, so OuterJoinRemover is constructed per optimization rather than shared.
        private readonly HashSet<ValueSlot> _nullRejected = new();

        protected override LogicalOperator RewriteFilter(LogicalFilter node)
        {
            // A filter rejecting the null-supplied side of a join below it is exactly what
            // drives the simplification, so record its rejections before descending.
            AddRejected(node.Conditions);
            return base.RewriteFilter(node);
        }

        protected override LogicalOperator RewriteJoin(LogicalJoin node)
        {
            node = Simplify(node);

            // Harvest rejection from this join's own condition for the subtree below. An
            // outer join's condition can't reject its preserved side (an unmatched row is
            // still emitted), so a full outer contributes nothing and a left outer only its
            // right; an inner/semi join contributes both sides.
            HarvestFromCondition(node);

            var rewritten = base.RewriteJoin(node);

            // Once the children are rewritten (and any outer join above has been tightened
            // to inner), drop a constant-relation side. Children are already simplified, so
            // this needs no re-run.
            return rewritten is LogicalJoin { JoinKind: LogicalJoinKind.Inner } inner ? RemoveConstantSide(inner) : rewritten;
        }

        // An inner join against the single-row, no-column constant relation is just the
        // other side, with the join condition (which can only reference that other side)
        // applied as a filter.
        private static LogicalOperator RemoveConstantSide(LogicalJoin node)
        {
            if (node.Left is LogicalConstant)
                return WrapWithFilter(node.Right, node.Conditions);

            if (node.Right is LogicalConstant)
                return WrapWithFilter(node.Left, node.Conditions);

            return node;
        }

        private static LogicalOperator WrapWithFilter(LogicalOperator input, ImmutableArray<LogicalExpression> conditions)
        {
            return conditions.IsEmpty ? input : new LogicalFilter(input, conditions);
        }

        private LogicalJoin Simplify(LogicalJoin node)
        {
            var leftRejected = AnyRejected(node.Left.DefinedValueSlots);
            var rightRejected = AnyRejected(node.Right.DefinedValueSlots);

            switch (node.JoinKind)
            {
                case LogicalJoinKind.LeftOuter when rightRejected:
                    return WithKind(node, LogicalJoinKind.Inner);

                case LogicalJoinKind.FullOuter when leftRejected && rightRejected:
                    return WithKind(node, LogicalJoinKind.Inner);

                case LogicalJoinKind.FullOuter when leftRejected:
                    return WithKind(node, LogicalJoinKind.LeftOuter);

                case LogicalJoinKind.FullOuter when rightRejected:
                    // Right-only rejection makes this a RIGHT OUTER, which the logical layer
                    // doesn't model -- so swap to the equivalent LEFT OUTER. Operands
                    // reference slots by identity, and a join's output column order is always
                    // re-established by the enclosing projection, so the swap is transparent
                    // (the same normalization the algebrizer applies to a RIGHT JOIN).
                    return new LogicalJoin(LogicalJoinKind.LeftOuter, node.Right, node.Left, node.Conditions, node.Probe, node.PassthruPredicate);

                default:
                    return node;
            }
        }

        private static LogicalJoin WithKind(LogicalJoin node, LogicalJoinKind kind)
        {
            return new LogicalJoin(kind, node.Left, node.Right, node.Conditions, node.Probe, node.PassthruPredicate);
        }

        private void HarvestFromCondition(LogicalJoin node)
        {
            if (node.JoinKind == LogicalJoinKind.FullOuter)
                return;

            var leftDefined = node.Left.DefinedValueSlots;
            var rightDefined = node.Right.DefinedValueSlots;

            // The left is preserved under a left outer join, so its condition rejections
            // don't constrain it; every other (inner/semi/anti) kind does.
            var harvestLeft = node.JoinKind != LogicalJoinKind.LeftOuter;

            foreach (var condition in node.Conditions)
            {
                foreach (var slot in LogicalSlotReferenceFinder.FindReferencedSlots(condition))
                {
                    if (!NullRejection.IsRejectingNull(condition, slot))
                        continue;

                    if (harvestLeft && leftDefined.Contains(slot))
                        _nullRejected.Add(slot);
                    else if (rightDefined.Contains(slot))
                        _nullRejected.Add(slot);
                }
            }
        }

        private void AddRejected(ImmutableArray<LogicalExpression> conditions)
        {
            foreach (var condition in conditions)
            {
                foreach (var slot in LogicalSlotReferenceFinder.FindReferencedSlots(condition))
                {
                    if (NullRejection.IsRejectingNull(condition, slot))
                        _nullRejected.Add(slot);
                }
            }
        }

        private bool AnyRejected(FrozenSet<ValueSlot> slots)
        {
            foreach (var slot in slots)
            {
                if (_nullRejected.Contains(slot))
                    return true;
            }

            return false;
        }
    }
}
