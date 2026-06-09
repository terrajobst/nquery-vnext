#nullable enable

using System.Collections.Immutable;

using NQuery.Algebra;
using NQuery.Binding;

namespace NQuery.LogicalOptimization
{
    // Normalizes and reorders inner-join regions. A connected region of inner joins
    // (cross products included) is collapsed into its set of leaf inputs plus a pool
    // of predicates -- the joins' own conditions, and, when a filter sits on top of
    // the region, that filter's conjuncts. The region is then rebuilt as a left-deep
    // tree, with each predicate attached to the lowest join whose inputs cover all
    // its slots. This is where a WHERE over cross products becomes equi-joins.
    //
    // Only inner joins reorder; outer/semi/anti joins bound the region -- their
    // conditions are not interchangeable with a WHERE -- so they are treated as
    // opaque inputs and optimized recursively. Ordering is heuristic (prefer an
    // input connected by a predicate, to avoid cartesian products); a cost-based
    // PickNext can replace it later.
    //
    // The pass rebuilds regions unconditionally, so it is not idempotent by
    // reference and must run in a Once batch rather than a fixed point.
    internal sealed class JoinOrderer : LogicalOperatorRewriter
    {
        public static readonly JoinOrderer Instance = new();

        private JoinOrderer()
        {
        }

        protected override LogicalOperator RewriteFilter(LogicalFilter node)
        {
            // A filter on top of an inner-join region: fold its conjuncts into the
            // predicate pool so they can become join conditions.
            if (IsRegionJoin(node.Input))
                return OrderRegion(node.Input, node.Conditions);

            return base.RewriteFilter(node);
        }

        protected override LogicalOperator RewriteJoin(LogicalJoin node)
        {
            if (IsRegionJoin(node))
                return OrderRegion(node, ImmutableArray<LogicalExpression>.Empty);

            return base.RewriteJoin(node);
        }

        private static bool IsRegionJoin(LogicalOperator node)
        {
            return node is LogicalJoin { JoinKind: LogicalJoinKind.Inner, Probe: null, PassthruPredicate: null };
        }

        private LogicalOperator OrderRegion(LogicalOperator regionRoot, ImmutableArray<LogicalExpression> extraPredicates)
        {
            var inputs = new List<LogicalOperator>();
            var predicates = new List<LogicalExpression>(extraPredicates);
            Collect(regionRoot, inputs, predicates);

            var remaining = new List<LogicalOperator>(inputs);
            var acc = remaining[0];
            remaining.RemoveAt(0);

            while (remaining.Count > 0)
            {
                var next = PickNext(acc, remaining, predicates);
                remaining.Remove(next);

                var available = new HashSet<ValueSlot>(acc.DefinedValueSlots);
                available.UnionWith(next.DefinedValueSlots);
                var conditions = TakeCoverable(predicates, available);

                acc = new LogicalJoin(LogicalJoinKind.Inner, acc, next, conditions, probe: null, passthruPredicate: null);
            }

            // Anything not coverable by the region (e.g. an outer reference) stays as
            // a filter on top.
            if (predicates.Count > 0)
                acc = new LogicalFilter(acc, predicates.ToImmutableArray());

            return acc;
        }

        private void Collect(LogicalOperator node, List<LogicalOperator> inputs, List<LogicalExpression> predicates)
        {
            if (node is LogicalJoin join && IsRegionJoin(join))
            {
                predicates.AddRange(join.Conditions);
                Collect(join.Left, inputs, predicates);
                Collect(join.Right, inputs, predicates);
            }
            else
            {
                // A region leaf: optimize it recursively (it may contain its own
                // regions, e.g. under an outer join), then treat it as a unit.
                inputs.Add(RewriteRelation(node));
            }
        }

        private static LogicalOperator PickNext(LogicalOperator acc, List<LogicalOperator> remaining, List<LogicalExpression> predicates)
        {
            var accSlots = acc.DefinedValueSlots;

            foreach (var candidate in remaining)
            {
                var candidateSlots = candidate.DefinedValueSlots;
                foreach (var predicate in predicates)
                {
                    var references = LogicalSlotReferenceFinder.FindReferencedSlots(predicate);
                    if (references.Any(accSlots.Contains) && references.Any(candidateSlots.Contains))
                        return candidate;
                }
            }

            // No connecting predicate: a cartesian product is unavoidable here.
            return remaining[0];
        }

        private static ImmutableArray<LogicalExpression> TakeCoverable(List<LogicalExpression> pool, HashSet<ValueSlot> available)
        {
            var taken = ImmutableArray.CreateBuilder<LogicalExpression>();

            for (var i = pool.Count - 1; i >= 0; i--)
            {
                var references = LogicalSlotReferenceFinder.FindReferencedSlots(pool[i]);
                if (references.All(available.Contains))
                {
                    taken.Add(pool[i]);
                    pool.RemoveAt(i);
                }
            }

            return taken.ToImmutable();
        }
    }
}
