using System.Collections.Frozen;
using System.Collections.Immutable;

using NQuery.CodeAnalysis.Algebra;

namespace NQuery.CodeAnalysis.Optimization;

// Normalizes and reorders inner-join regions. A connected region of inner joins
// (cross products included) is collapsed into its set of leaf inputs plus a pool
// of predicates -- the joins' own conditions, and, when a filter sits on top of
// the region, that filter's conjuncts. The region is then rebuilt as a left-deep
// tree, with each predicate attached to the lowest join whose inputs cover all
// its slots. This is where a WHERE over cross products becomes equi-joins.
//
// Only inner joins reorder; outer/semi/anti joins bound the region -- their
// conditions are not interchangeable with a WHERE -- so they are treated as
// opaque inputs and optimized recursively. The one exception is a left outer join
// whose null-supplied side no region predicate references: its preserved side is
// pulled into the region (so it can join and reorder with the rest) and the outer
// join re-applied above (see PullUpOuterJoins). Ordering is heuristic (prefer an input connected by a
// predicate, to avoid cartesian products); a cost-based PickNext can replace it later.
//
// The pass rebuilds regions unconditionally, so it is not idempotent by
// reference and must run in a Once batch rather than a fixed point.
internal sealed class JoinOrderer : LogicalOperatorRewriter
{
    public static JoinOrderer Instance { get; } = new();

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
            return OrderRegion(node, []);

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

        var deferred = PullUpOuterJoins(inputs, predicates);

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
        // a filter on top. It can't reference a deferred null-supplied side -- that is
        // exactly what PullUpOuterJoins guards against -- so it is safe below them.
        if (predicates.Count > 0)
            acc = new LogicalFilter(acc, [.. predicates]);

        // Re-apply the deferred outer joins, innermost (most recently peeled) first, so a
        // nested chain is rebuilt in its original order.
        for (var i = deferred.Count - 1; i >= 0; i--)
            acc = new LogicalJoin(LogicalJoinKind.LeftOuter, acc, deferred[i].NullSupplied, deferred[i].Conditions, probe: null, passthruPredicate: null);

        return acc;
    }

    // Re-association across an inner/outer boundary: R ⋈ₚ (L ⟕_q M) == (R ⋈ₚ L) ⟕_q M,
    // valid when no region predicate references the null-supplied side M (so L can join
    // and reorder with the rest of the region while M is re-attached above). Each peeled
    // outer join is returned to be re-applied over the assembled region; the loop repeats
    // so a nested outer join ((D ⟕ E) ⟕ C) peels one level per pass.
    private static List<DeferredOuterJoin> PullUpOuterJoins(List<LogicalOperator> inputs, List<LogicalExpression> predicates)
    {
        var deferred = new List<DeferredOuterJoin>();

        bool changed;
        do
        {
            changed = false;

            for (var i = 0; i < inputs.Count; i++)
            {
                if (inputs[i] is LogicalJoin { JoinKind: LogicalJoinKind.LeftOuter, Probe: null, PassthruPredicate: null } outer &&
                    !AnyPredicateReferences(predicates, outer.Right.DefinedValueSlots))
                {
                    inputs[i] = outer.Left;
                    deferred.Add(new DeferredOuterJoin(outer.Right, outer.Conditions));
                    changed = true;
                }
            }
        }
        while (changed);

        return deferred;
    }

    private static bool AnyPredicateReferences(List<LogicalExpression> predicates, FrozenSet<ValueSlot> slots)
    {
        foreach (var predicate in predicates)
        {
            foreach (var slot in LogicalSlotReferenceFinder.FindReferencedSlots(predicate))
            {
                if (slots.Contains(slot))
                    return true;
            }
        }

        return false;
    }

    private readonly struct DeferredOuterJoin
    {
        public DeferredOuterJoin(LogicalOperator nullSupplied, ImmutableArray<LogicalExpression> conditions)
        {
            NullSupplied = nullSupplied;
            Conditions = conditions;
        }

        // The null-supplied (right) side of the peeled left outer join.
        public LogicalOperator NullSupplied { get; }

        // The peeled join's own condition (references the preserved side, now in the
        // region, and the null-supplied side).
        public ImmutableArray<LogicalExpression> Conditions { get; }
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
