#nullable enable

using System.Collections.Immutable;

using NQuery.Algebra;
using NQuery.Binding;

namespace NQuery.Optimization
{
    // Pushes selection conjuncts into the side of an inner join that already
    // defines every slot a conjunct references, recursing so a conjunct travels as
    // far down a chain of joins as it can. Because a filter's predicate is stored
    // as a list of conjuncts, each conjunct is placed independently: some can move
    // to the left, some to the right, and any that reference both sides stay above
    // the join.
    //
    // Restricted to inner joins for now: pushing a predicate into the
    // null-producing side of an outer join changes its meaning. Pushing through
    // projects/computes is a future refinement.
    internal sealed class SelectionPushdown : LogicalOperatorRewriter
    {
        public static readonly SelectionPushdown Instance = new();

        private SelectionPushdown()
        {
        }

        protected override LogicalOperator RewriteFilter(LogicalFilter node)
        {
            var rewritten = base.RewriteFilter(node);
            return rewritten is LogicalFilter filter ? PushDown(filter) : rewritten;
        }

        private static LogicalOperator PushDown(LogicalFilter filter)
        {
            if (filter.Input is not LogicalJoin { JoinKind: LogicalJoinKind.Inner } join)
                return filter;

            var toLeft = ImmutableArray.CreateBuilder<LogicalExpression>();
            var toRight = ImmutableArray.CreateBuilder<LogicalExpression>();
            var stay = ImmutableArray.CreateBuilder<LogicalExpression>();

            foreach (var conjunct in filter.Conditions)
            {
                var references = LogicalSlotReferenceFinder.FindReferencedSlots(conjunct);
                if (References(references, join.Left))
                    toLeft.Add(conjunct);
                else if (References(references, join.Right))
                    toRight.Add(conjunct);
                else
                    stay.Add(conjunct);
            }

            if (toLeft.Count == 0 && toRight.Count == 0)
                return filter;

            var left = toLeft.Count == 0 ? join.Left : PushDown(new LogicalFilter(join.Left, toLeft.ToImmutable()));
            var right = toRight.Count == 0 ? join.Right : PushDown(new LogicalFilter(join.Right, toRight.ToImmutable()));
            var newJoin = new LogicalJoin(join.JoinKind, left, right, join.Conditions, join.Probe, join.PassthruPredicate);

            return stay.Count == 0 ? newJoin : new LogicalFilter(newJoin, stay.ToImmutable());
        }

        private static bool References(ISet<ValueSlot> slots, LogicalOperator relation)
        {
            var defined = relation.DefinedValueSlots;
            return slots.All(defined.Contains);
        }
    }
}
