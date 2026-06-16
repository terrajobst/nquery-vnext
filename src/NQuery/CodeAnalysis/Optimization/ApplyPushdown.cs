using System.Collections;
using System.Collections.Immutable;

using NQuery.CodeAnalysis.Algebra;
using NQuery.CodeAnalysis.Binding;
using NQuery.Metadata.Aggregation;

namespace NQuery.CodeAnalysis.Optimization;

// Decorrelation by pushing Apply down: an Apply whose right side no longer
// depends on the left becomes an ordinary join. The rules so far:
//
//   * base case -- Apply(L, R) with R independent of L  ->  Join(L, R) (no
//     condition); this fully decorrelates uncorrelated subqueries.
//   * project       -- pushed through (dropped for a semi apply, which ignores
//     the right's columns).
//   * compute       -- pushed through for Inner (dropped for semi). NOT for a
//     LeftOuter apply: lifting a compute above it would evaluate the computed
//     expression over the null-padded row rather than suppress it.
//   * correlated filter -- the correlated predicate becomes the join condition
//     once the rest of the right side is independent of the left.
//   * scalar aggregate  -- a correlated scalar aggregate subquery is decorrelated
//     via DecorrelateAggregate (count-bug safe; see there).
//
// Anything not matched is left as an Apply -- still correct (executable as a
// correlated dependent join).
//
// TODO: This is a pattern-matched pushdown. The general technique -- Neumann &
//       Kemper's "Unnesting Arbitrary Queries" (a Domain operator pushed to the
//       leaves) -- would subsume every rule here and decorrelate shapes the
//       patterns miss (e.g. correlation buried under a join). DecorrelateAggregate
//       already builds a domain ad hoc; generalizing it is the natural next step.
internal sealed class ApplyPushdown : LogicalOperatorRewriter
{
    private readonly Func<Type, IComparer?> _comparerResolver;

    // The comparer resolver is the engine's per-type comparer (DataContext-registered,
    // else Comparer.Default) -- needed because the aggregate-decorrelation domain groups
    // on the correlation keys, and that grouping must partition keys exactly as the
    // join-back equality does.
    public ApplyPushdown(Func<Type, IComparer?> comparerResolver)
    {
        _comparerResolver = comparerResolver;
    }

    protected override LogicalOperator RewriteApply(LogicalApply node)
    {
        // Rewrite children first, so nested applies are decorrelated bottom-up.
        var rewritten = base.RewriteApply(node);
        return rewritten is LogicalApply apply ? Decorrelate(apply) : rewritten;
    }

    private LogicalOperator Decorrelate(LogicalApply apply)
    {
        // A guarded apply (a CASE-branch subquery whose evaluation is conditional)
        // is left as nested loops: the executor's passthru handling skips the right
        // for guarded rows, which a decorrelated join would lose.
        if (apply.Passthru is not null)
            return apply;

        // Base case: no outer references, so the right no longer reaches into the
        // left -- it is already an ordinary join.
        if (apply.OuterReferences.IsEmpty)
            return ToJoin(apply, apply.Right, ImmutableArray<LogicalExpression>.Empty);

        switch (apply.Right)
        {
            // An existence test ignores the right's columns, so a project there
            // is irrelevant -- drop it and continue.
            case LogicalProject project when IsSemi(apply.ApplyKind):
                return Decorrelate(new LogicalApply(apply.ApplyKind, apply.Left, project.Input, apply.Probe));

            // Otherwise push the apply below the project, keeping the left's
            // columns in scope (Apply(L, π(R)) == π(Apply(L, R)) over L ∪ outs).
            case LogicalProject project:
                var inner = Decorrelate(new LogicalApply(apply.ApplyKind, apply.Left, project.Input, apply.Probe));
                var outputs = apply.Left.OutputValueSlots.Concat(project.Outputs).ToImmutableArray();
                return new LogicalProject(inner, outputs);

            // An existence test ignores the computed columns -- drop the compute.
            case LogicalCompute compute when IsSemi(apply.ApplyKind):
                return Decorrelate(new LogicalApply(apply.ApplyKind, apply.Left, compute.Input, apply.Probe));

            // Inner apply: a compute lifts above the (now uncorrelated) join unchanged,
            // since the inner apply pads nothing (Apply(L, χ(R)) == χ(Apply(L, R))). A
            // LeftOuter apply is intentionally absent here -- see the class comment.
            case LogicalCompute compute when apply.ApplyKind == LogicalApplyKind.Inner:
                var computeInner = Decorrelate(new LogicalApply(apply.ApplyKind, apply.Left, compute.Input, apply.Probe));
                return new LogicalCompute(computeInner, compute.DefinedValues);

            // The correlated predicate becomes the join condition once the rest
            // of the right side is independent of the left.
            case LogicalFilter filter when !DependsOnLeft(filter.Input, apply.Left):
                return ToJoin(apply, filter.Input, filter.Conditions);

            // A correlated scalar aggregate subquery (no GROUP BY of its own). Only the
            // LeftOuter shape the algebrizer emits for a scalar subquery is handled here.
            case LogicalAggregate aggregate when apply.ApplyKind == LogicalApplyKind.LeftOuter && aggregate.Groups.IsEmpty:
                return DecorrelateAggregate(apply, aggregate);

            // TODO: Push Apply into one side of an inner Join on the right (the
            //       cross-join predicate movement), and handle a subquery
            //       with its own GROUP BY (aggregate.Groups non-empty) and the
            //       Assert-guarded multi-row scalar case. Until then these stay nested
            //       loops -- correct, but a re-scan per left row.
            default:
                return apply;
        }
    }

    // Decorrelates  Apply_LeftOuter(L, Γ_{∅; F}(Rbody))  where Rbody is correlated to L.
    //
    // Naively turning the apply into a join and grouping hits the "count bug": an L row
    // whose subquery matches nothing must still yield COUNT = 0 -- not NULL, and not the 1
    // a null-padded row would contribute. This construction avoids it without a marker:
    //
    //   * Build a domain D = the distinct correlation keys of (a clone of) L.
    //   * INNER-join D to the decorrelated body on the correlation and aggregate per key.
    //     A key with no matching body rows simply produces no group.
    //   * LEFT-OUTER-join L back to the per-key aggregates on the correlation keys, then
    //     map each aggregate output through COALESCE(value, <empty-group value>). The
    //     empty-group value is read straight from the aggregator (COUNT -> 0, the rest ->
    //     NULL), so an absent group restores exactly what the subquery yields over no rows.
    //
    // The domain is a whole clone of L (re-scanned); pushing it down to the correlated
    // leaves -- the Neumann magic-set refinement -- is future work (see the class comment).
    private LogicalOperator DecorrelateAggregate(LogicalApply apply, LogicalAggregate aggregate)
    {
        var left = apply.Left;
        var correlation = apply.OuterReferences;

        // The domain groups on the correlation keys, so each key type needs the engine's
        // comparer. If any is missing, leave the apply as a (correct) dependent join.
        var comparers = new IComparer[correlation.Length];
        for (var i = 0; i < correlation.Length; i++)
        {
            var comparer = _comparerResolver(correlation[i].Type);
            if (comparer is null)
                return apply;
            comparers[i] = comparer;
        }

        // Read each aggregate's empty-group value now (COUNT -> 0, others -> NULL). If an
        // aggregator can't be evaluated over zero rows, fall back to nested loops.
        var emptyValues = new object?[aggregate.Aggregates.Length];
        for (var i = 0; i < aggregate.Aggregates.Length; i++)
        {
            if (!TryGetEmptyValue(aggregate.Aggregates[i].Aggregatable, out emptyValues[i]))
                return apply;
        }

        // Domain: distinct correlation keys over a slot-disjoint clone of L. Cloning keeps
        // L's own slots free for the join-back below; the clone's keys line up with the
        // originals positionally (Clone preserves output order).
        var cloner = new LogicalOperatorCloner();
        var clonedLeft = cloner.Clone(left);
        var domainKeys = new ValueSlot[correlation.Length];
        for (var i = 0; i < correlation.Length; i++)
        {
            var index = left.OutputValueSlots.IndexOf(correlation[i]);
            domainKeys[i] = clonedLeft.OutputValueSlots[index];
        }

        var domain = new LogicalAggregate(clonedLeft, BuildGroups(domainKeys, comparers), ImmutableArray<LogicalAggregatedValue>.Empty);

        // The body, with its correlation references remapped onto the domain's keys (the
        // same cloner already mapped them while cloning L). Decorrelating an *inner* apply
        // turns the correlation into the join condition; any residue stays a (correct)
        // dependent join.
        var clonedBody = cloner.Clone(aggregate.Input);
        var matched = Decorrelate(new LogicalApply(LogicalApplyKind.Inner, domain, clonedBody, probe: null));

        // Per-key aggregates into fresh output slots; arguments follow the cloned body.
        var perKeyAggregates = aggregate.Aggregates
                                        .Select(a => new LogicalAggregatedValue(a.Output.Duplicate(), a.Aggregate, a.Aggregatable, cloner.CloneExpression(a.Argument)))
                                        .ToImmutableArray();
        var perKey = new LogicalAggregate(matched, BuildGroups(domainKeys, comparers), perKeyAggregates);

        // Join L back to its per-key aggregates, null-safe so a NULL correlation key
        // matches its NULL group. Every L key is in the domain by construction, so a left
        // outer is enough to keep the keys whose group was empty (NULL aggregate outputs).
        var backConditions = ImmutableArray.CreateRange(
            Enumerable.Range(0, correlation.Length)
                      .Select(i => NullSafeEquality(correlation[i], domainKeys[i])));
        var back = new LogicalJoin(LogicalJoinKind.LeftOuter, left, perKey, backConditions, probe: null, passthruPredicate: null);

        // Restore the original aggregate output slots, applying the empty-group value to
        // keys that had no group (NULL after the outer join).
        var restored = aggregate.Aggregates
                                .Select((a, i) => new LogicalComputedValue(RestoreEmpty(perKeyAggregates[i].Output, emptyValues[i]), a.Output))
                                .ToImmutableArray();
        var computed = new LogicalCompute(back, restored);

        // Drop the domain keys so the output matches the original apply (L ++ aggregates).
        var projected = left.OutputValueSlots.Concat(aggregate.OutputValueSlots).ToImmutableArray();
        return new LogicalProject(computed, projected);
    }

    // The aggregate's result over zero rows, by running the aggregator with no
    // accumulations (COUNT -> 0, the rest -> NULL). A throwing aggregator signals "can't
    // decorrelate safely".
    private static bool TryGetEmptyValue(IAggregatable aggregatable, out object? emptyValue)
    {
        try
        {
            var aggregator = aggregatable.CreateAggregator();
            aggregator.Initialize();
            emptyValue = aggregator.GetResult();
            return true;
        }
        catch
        {
            emptyValue = null;
            return false;
        }
    }

    // COALESCE(value, empty): the per-key value when the group exists, else the aggregate's
    // empty-group value. A NULL empty value (everything but COUNT) needs no guard.
    private static LogicalExpression RestoreEmpty(ValueSlot value, object? emptyValue)
    {
        var reference = new LogicalValueSlotExpression(value);
        if (emptyValue is null)
            return reference;

        var label = new LogicalCaseLabel(new LogicalIsNullExpression(reference), new LogicalLiteralExpression(emptyValue));
        return new LogicalCaseExpression(ImmutableArray.Create(label), reference);
    }

    private static ImmutableArray<LogicalComparedValue> BuildGroups(ValueSlot[] keys, IComparer[] comparers)
    {
        var builder = ImmutableArray.CreateBuilder<LogicalComparedValue>(keys.Length);
        for (var i = 0; i < keys.Length; i++)
            builder.Add(new LogicalComparedValue(keys[i], comparers[i]));
        return builder.MoveToImmutable();
    }

    // (l = r) OR (l IS NULL AND r IS NULL) -- mirrors Planner.BuildNullSafeEquality, so two
    // NULL keys match (plain `=` yields NULL, i.e. no match).
    private static LogicalExpression NullSafeEquality(ValueSlot left, ValueSlot right)
    {
        var leftExpr = new LogicalValueSlotExpression(left);
        var rightExpr = new LogicalValueSlotExpression(right);
        var equal = Binary(leftExpr, BinaryOperatorKind.Equal, rightExpr);
        var bothNull = Binary(new LogicalIsNullExpression(leftExpr), BinaryOperatorKind.LogicalAnd, new LogicalIsNullExpression(rightExpr));
        return Binary(equal, BinaryOperatorKind.LogicalOr, bothNull);
    }

    private static LogicalBinaryExpression Binary(LogicalExpression left, BinaryOperatorKind kind, LogicalExpression right)
    {
        var result = BinaryOperator.Resolve(kind, left.Type, right.Type);
        return new LogicalBinaryExpression(left, kind, result, right);
    }

    private static bool IsSemi(LogicalApplyKind kind)
    {
        return kind is LogicalApplyKind.LeftSemi or LogicalApplyKind.LeftAntiSemi;
    }

    private static bool DependsOnLeft(LogicalOperator right, LogicalOperator left)
    {
        var references = LogicalSlotReferenceFinder.FindReferencedSlots(right);
        return left.DefinedValueSlots.Any(references.Contains);
    }

    private static LogicalOperator ToJoin(LogicalApply apply, LogicalOperator right, ImmutableArray<LogicalExpression> conditions)
    {
        var joinKind = apply.ApplyKind switch
        {
            LogicalApplyKind.Inner => LogicalJoinKind.Inner,
            LogicalApplyKind.LeftOuter => LogicalJoinKind.LeftOuter,
            LogicalApplyKind.LeftSemi => LogicalJoinKind.LeftSemi,
            LogicalApplyKind.LeftAntiSemi => LogicalJoinKind.LeftAntiSemi,
            _ => throw ExceptionBuilder.UnexpectedValue(apply.ApplyKind)
        };

        return new LogicalJoin(joinKind, apply.Left, right, conditions, apply.Probe, passthruPredicate: null);
    }
}
