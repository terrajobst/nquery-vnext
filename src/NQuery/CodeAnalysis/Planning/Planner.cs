using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;

using NQuery.CodeAnalysis.Algebra;
using NQuery.CodeAnalysis.Binding;

namespace NQuery.CodeAnalysis.Planning;

// Translates the optimized logical algebra into a physical operator tree
// (Logical -> Physical). Currently a one-to-one lowering: every join becomes
// nested loops. Algorithm selection (a sibling hash-match node for equi-joins),
// the stream-vs-hash aggregate choice, and cost-based decisions are future work.
internal static class Planner
{
    public static PhysicalQuery Plan(LogicalQuery query)
    {
        ThrowIfNull(query);

        var root = Plan(query.Root);
        return new PhysicalQuery(root, query.OutputColumns);
    }

    public static PhysicalOperator Plan(LogicalOperator root)
    {
        ThrowIfNull(root);

        return PlanOperator(root, []);
    }

    private static PhysicalOperator PlanOperator(LogicalOperator node, ImmutableArray<ValueSlot> outerSlots)
    {
        switch (node.Kind)
        {
            case LogicalOperatorKind.Empty:
                return PlanEmpty((LogicalEmpty)node);
            case LogicalOperatorKind.Constant:
                return PlanConstant((LogicalConstant)node);
            case LogicalOperatorKind.TableScan:
                return PlanTableScan((LogicalTableScan)node);
            case LogicalOperatorKind.Filter:
                return PlanFilter((LogicalFilter)node, outerSlots);
            case LogicalOperatorKind.Compute:
                return PlanCompute((LogicalCompute)node, outerSlots);
            case LogicalOperatorKind.Project:
                return PlanProject((LogicalProject)node, outerSlots);
            case LogicalOperatorKind.Join:
                return PlanJoin((LogicalJoin)node, outerSlots);
            case LogicalOperatorKind.Apply:
                return PlanApply((LogicalApply)node, outerSlots);
            case LogicalOperatorKind.Aggregate:
                return PlanStreamAggregates((LogicalAggregate)node, outerSlots);
            case LogicalOperatorKind.Union:
                return PlanUnion((LogicalUnion)node, outerSlots);
            case LogicalOperatorKind.RecursiveUnion:
                return PlanRecursiveUnion((LogicalRecursiveUnion)node, outerSlots);
            case LogicalOperatorKind.RecursiveReference:
                return PlanRecursiveReference((LogicalRecursiveReference)node);
            case LogicalOperatorKind.IntersectOrExcept:
                return PlanIntersectOrExcept((LogicalIntersectOrExcept)node, outerSlots);
            case LogicalOperatorKind.Sort:
                return PlanSort((LogicalSort)node, outerSlots);
            case LogicalOperatorKind.Top:
                return PlanTop((LogicalTop)node, outerSlots);
            case LogicalOperatorKind.Assert:
                return PlanAssert((LogicalAssert)node, outerSlots);
            default:
                throw ExceptionBuilder.UnexpectedValue(node.Kind);
        }
    }

    private static PhysicalOperator PlanEmpty(LogicalEmpty node) => new PhysicalEmpty();

    private static PhysicalOperator PlanConstant(LogicalConstant node) => new PhysicalConstant();

    private static PhysicalOperator PlanTableScan(LogicalTableScan node) => new PhysicalTableScan(node.TableInstance, node.DefinedValues, node.OutputValueSlots);

    private static PhysicalOperator PlanFilter(LogicalFilter node, ImmutableArray<ValueSlot> outerSlots)
    {
        // A correlated filter sits inside an apply's right side, so it is re-evaluated
        // per outer row -- and so is its whole input subtree. When the correlation is an
        // equality against an outer slot and the input is outer-independent, an index
        // spool turns that O(outer x inner) re-scan into one scan plus a per-row probe.
        if (!outerSlots.IsEmpty && TryPlanIndexSpool(node, outerSlots, out var indexSpool))
            return indexSpool;

        var input = PlanOperator(node.Input, outerSlots);
        return new PhysicalFilter(input, node.Conditions);
    }

    // Lowers a correlated filter to an index spool: the input is scanned once, indexed
    // by the equality's inner side, and probed by the outer slot on every re-open --
    // essentially SQL Server's lazy spool. Structural requirements:
    //
    //   * some conjunct is an equality between a plain outer slot (the probe) and a
    //     value computable from the input alone (the index key);
    //   * the input references no outer slot -- otherwise its content changes with the
    //     outer row and a build-once spool would serve stale rows (the eager-spool
    //     case, which we don't attempt: such filters just stay correlated filters); and
    //   * the input reads no recursive CTE working table, whose content changes per
    //     recursion round the same way.
    //
    // The other conjuncts survive around the spool: outer-independent ones move below
    // it (evaluated once, while spooling), the rest stay above it (evaluated per row on
    // the probed subset) -- including any further correlated equalities.
    private static bool TryPlanIndexSpool(LogicalFilter node, ImmutableArray<ValueSlot> outerSlots, out PhysicalOperator result)
    {
        result = null!;

        var inputSlots = node.Input.OutputValueSlots;
        if (!TryFindSpoolKey(node.Conditions, inputSlots, outerSlots, out var key))
            return false;

        var referenced = LogicalSlotReferenceFinder.FindReferencedSlots(node.Input);
        if (outerSlots.Any(referenced.Contains))
            return false;

        if (node.Input.DescendantsAndSelf().Any(d => d.Kind == LogicalOperatorKind.RecursiveReference))
            return false;

        var remainder = node.Conditions.RemoveAt(key.ConditionIndex);
        var below = remainder.RemoveAll(c => LogicalSlotReferenceFinder.FindReferencedSlots(c).Any(outerSlots.Contains));
        var above = remainder.RemoveRange(below);

        var input = PlanOperator(node.Input, outerSlots);
        if (!below.IsEmpty)
            input = new PhysicalFilter(input, below);
        if (key.IndexComputedValue is not null)
            input = new PhysicalComputeScalar(input, [key.IndexComputedValue]);

        result = new PhysicalIndexSpool(input, key.IndexKey, key.ProbeKey);

        // A computed index key leaks an extra slot into the spool's output; project
        // back onto the filter's own outputs to drop it (same as the hash match).
        if (key.IndexComputedValue is not null)
            result = new PhysicalProject(result, node.OutputValueSlots);

        if (!above.IsEmpty)
            result = new PhysicalFilter(result, above);

        return true;
    }

    // Finds a conjunct usable as the spool key: an equality whose one side is a plain
    // outer slot (the probe -- evaluated against the outer row on every re-open) and
    // whose other side is over the input only (the index key). Like the hash match's
    // equi-key, a plain slot = slot equality is preferred; a computed inner side is
    // materialized by a compute below the spool. A computed *outer* side has no input
    // to attach a compute to, so it disqualifies the conjunct.
    private static bool TryFindSpoolKey(ImmutableArray<LogicalExpression> conditions, ImmutableArray<ValueSlot> inputSlots, ImmutableArray<ValueSlot> outerSlots, out SpoolKey key)
    {
        return TryFindSpoolKey(conditions, inputSlots, outerSlots, plainOnly: true, out key) ||
               TryFindSpoolKey(conditions, inputSlots, outerSlots, plainOnly: false, out key);
    }

    private static bool TryFindSpoolKey(ImmutableArray<LogicalExpression> conditions, ImmutableArray<ValueSlot> inputSlots, ImmutableArray<ValueSlot> outerSlots, bool plainOnly, out SpoolKey key)
    {
        for (var i = 0; i < conditions.Length; i++)
        {
            if (conditions[i] is not LogicalBinaryExpression { OperatorKind: BinaryOperatorKind.Equal } equal)
                continue;

            LogicalExpression indexSide;
            ValueSlot probeKey;
            if (equal.Left is LogicalValueSlotExpression { ValueSlot: var leftSlot } && outerSlots.Contains(leftSlot) && ReferencesOnly(equal.Right, inputSlots))
            {
                indexSide = equal.Right;
                probeKey = leftSlot;
            }
            else if (equal.Right is LogicalValueSlotExpression { ValueSlot: var rightSlot } && outerSlots.Contains(rightSlot) && ReferencesOnly(equal.Left, inputSlots))
            {
                indexSide = equal.Left;
                probeKey = rightSlot;
            }
            else
            {
                continue;
            }

            if (plainOnly && indexSide is not LogicalValueSlotExpression)
                continue;

            var indexKey = AsKeySlot(indexSide, out var indexComputedValue);
            key = new SpoolKey(i, indexKey, probeKey, indexComputedValue);
            return true;
        }

        key = default;
        return false;
    }

    private readonly struct SpoolKey
    {
        public SpoolKey(int conditionIndex, ValueSlot indexKey, ValueSlot probeKey, LogicalComputedValue? indexComputedValue)
        {
            ConditionIndex = conditionIndex;
            IndexKey = indexKey;
            ProbeKey = probeKey;
            IndexComputedValue = indexComputedValue;
        }

        // Which conjunct is the key equality (it is consumed by the spool).
        public int ConditionIndex { get; }

        public ValueSlot IndexKey { get; }

        public ValueSlot ProbeKey { get; }

        // The compute that materializes the index key, or null when it is a plain slot.
        public LogicalComputedValue? IndexComputedValue { get; }
    }

    private static PhysicalOperator PlanCompute(LogicalCompute node, ImmutableArray<ValueSlot> outerSlots)
    {
        var input = PlanOperator(node.Input, outerSlots);
        return new PhysicalComputeScalar(input, node.DefinedValues);
    }

    private static PhysicalOperator PlanProject(LogicalProject node, ImmutableArray<ValueSlot> outerSlots)
    {
        var input = PlanOperator(node.Input, outerSlots);
        return new PhysicalProject(input, node.Outputs);
    }

    private static PhysicalOperator PlanJoin(LogicalJoin node, ImmutableArray<ValueSlot> outerSlots)
    {
        // Prefer a hash match for an equi-join it supports (inner / left outer / full
        // outer / left semi / left anti-semi). It produces FULL OUTER directly, so an
        // equi full outer skips the nested-loops expansion below; an equi EXISTS /
        // NOT EXISTS (a probing semi join) likewise beats the nested-loops re-scan.
        //
        // TODO: Build is always the join's left -- there is no cost model, so we don't
        //       choose the smaller input to hash, nor decide hash-vs-loops by estimated
        //       cardinality. Once cardinality estimation exists, pick the build side
        //       (and the algorithm) by cost.
        if (CanHashMatch(node.JoinKind) && TryGetEquiKey(node, out var key))
        {
            var build = PlanOperator(node.Left, outerSlots);
            var probe = PlanOperator(node.Right, outerSlots);

            // A computed key (e.g. ON a.x + 1 = b.y) isn't a slot the hash table can key
            // on, so materialize it with a compute on that input. Doing it here, rather
            // than as a logical rewrite, keeps the nested-loops fallback's predicate
            // inline -- only the chosen hash match pays for the compute. The extra slot
            // leaks into the hash match's output, so a projection back onto the join's
            // own outputs drops it -- there is not always an enclosing projection to do
            // that (ProjectMerger removes one that became an identity, e.g. over inlined
            // CTE references), and a leaked slot would widen the query's output row.
            if (key.BuildComputedValue is not null)
                build = new PhysicalComputeScalar(build, [key.BuildComputedValue]);
            if (key.ProbeComputedValue is not null)
                probe = new PhysicalComputeScalar(probe, [key.ProbeComputedValue]);

            // The recursive step: the probe side is the frontier (a recursive reference) while
            // the build side is not, and the build references no outer row -- so the build is
            // the same on every round's re-open and may be built once. (Only the recursive step
            // is checked, so non-recursion planning skips the slot walks.)
            var buildInvariant = ReferencesRecursiveReference(node.Right)
                                 && !ReferencesRecursiveReference(node.Left)
                                 && !ReferencesAny(node.Left, outerSlots);

            var hashMatch = new PhysicalHashMatch(MapHashMatchKind(node.JoinKind), build, probe, key.BuildKey, key.ProbeKey, key.Remainder, node.Probe, buildInvariant);
            return key.BuildComputedValue is null && key.ProbeComputedValue is null
                ? hashMatch
                : new PhysicalProject(hashMatch, node.OutputValueSlots);
        }

        // Nested loops can't produce a full outer join. Expanding it into operators
        // that can is a planning-time strategy choice, so it happens here rather than
        // in the algebra: the logical tree keeps the FULL OUTER as a single join.
        if (node.JoinKind == LogicalJoinKind.FullOuter)
            return PlanOperator(ExpandFullOuterJoin(node), outerSlots);

        var left = PlanOperator(node.Left, outerSlots);
        var right = PlanOperator(node.Right, outerSlots);
        return new PhysicalNestedLoops(MapJoinKind(node.JoinKind), left, right, node.Conditions, node.Probe, node.PassthruPredicate, []);
    }

    // A hash match can serve inner / left outer / full outer, and -- by hashing the
    // build (left) side and emitting or suppressing each build row on whether the
    // probe matched -- left semi / left anti-semi too. So an equi EXISTS / NOT EXISTS
    // becomes a hash match rather than a (probing) nested-loops re-scan.
    private static bool CanHashMatch(LogicalJoinKind kind)
    {
        return kind is LogicalJoinKind.Inner
                    or LogicalJoinKind.LeftOuter
                    or LogicalJoinKind.FullOuter
                    or LogicalJoinKind.LeftSemi
                    or LogicalJoinKind.LeftAntiSemi;
    }

    private static PhysicalHashMatchKind MapHashMatchKind(LogicalJoinKind kind)
    {
        return kind switch
        {
            LogicalJoinKind.Inner => PhysicalHashMatchKind.Inner,
            LogicalJoinKind.LeftOuter => PhysicalHashMatchKind.LeftOuter,
            LogicalJoinKind.FullOuter => PhysicalHashMatchKind.FullOuter,
            LogicalJoinKind.LeftSemi => PhysicalHashMatchKind.LeftSemi,
            LogicalJoinKind.LeftAntiSemi => PhysicalHashMatchKind.LeftAntiSemi,
            _ => throw ExceptionBuilder.UnexpectedValue(kind)
        };
    }

    // Finds a conjunct that is a cross-input equality -- one side over only the left
    // input, the other over only the right -- to use as the hash key, with the rest as
    // the residual remainder. A plain slot side is the key directly; a computed side
    // (ON a.x + 1 = b.y) is paired with the compute that materializes it. A plain
    // slot = slot key is preferred, so a computed key is only chosen when no plain one
    // exists. A non-equi, constant, mixed, or single-sided condition yields no key, so
    // the caller falls back to nested loops.
    private static bool TryGetEquiKey(LogicalJoin node, out EquiKey key)
    {
        var leftSlots = node.Left.OutputValueSlots;
        var rightSlots = node.Right.OutputValueSlots;

        return TryFindEquiKey(node, leftSlots, rightSlots, plainOnly: true, out key) ||
               TryFindEquiKey(node, leftSlots, rightSlots, plainOnly: false, out key);
    }

    private static bool TryFindEquiKey(LogicalJoin node, ImmutableArray<ValueSlot> leftSlots, ImmutableArray<ValueSlot> rightSlots, bool plainOnly, out EquiKey key)
    {
        for (var i = 0; i < node.Conditions.Length; i++)
        {
            if (node.Conditions[i] is LogicalBinaryExpression { OperatorKind: BinaryOperatorKind.Equal } equal &&
                TryOrientSides(equal, leftSlots, rightSlots, out var buildSide, out var probeSide))
            {
                var isPlain = buildSide is LogicalValueSlotExpression && probeSide is LogicalValueSlotExpression;
                if (plainOnly && !isPlain)
                    continue;

                var buildKey = AsKeySlot(buildSide, out var buildComputedValue);
                var probeKey = AsKeySlot(probeSide, out var probeComputedValue);
                key = new EquiKey(buildKey, probeKey, buildComputedValue, probeComputedValue, node.Conditions.RemoveAt(i));
                return true;
            }
        }

        key = default;
        return false;
    }

    // Orients an equality so buildSide is over the left input only and probeSide over the
    // right input only (swapping if it was written the other way). Fails for a constant,
    // mixed, or single-input equality -- none of which is a usable hash key.
    //
    // TODO: This treats the `=` operator as satisfiable by the hash match's runtime key
    //       equality (Dictionary<object>, i.e. object.Equals/GetHashCode). That holds for
    //       the current value types, but a type whose `=` operator disagrees with object
    //       equality (e.g. a culture-sensitive string compare) would make the hash match
    //       diverge from nested loops. Guard by the key type's comparer, or thread the
    //       operator's comparer into the hash table.
    private static bool TryOrientSides(LogicalBinaryExpression equal, ImmutableArray<ValueSlot> leftSlots, ImmutableArray<ValueSlot> rightSlots, out LogicalExpression buildSide, out LogicalExpression probeSide)
    {
        if (ReferencesOnly(equal.Left, leftSlots) && ReferencesOnly(equal.Right, rightSlots))
        {
            buildSide = equal.Left;
            probeSide = equal.Right;
            return true;
        }

        if (ReferencesOnly(equal.Left, rightSlots) && ReferencesOnly(equal.Right, leftSlots))
        {
            buildSide = equal.Right;
            probeSide = equal.Left;
            return true;
        }

        buildSide = null!;
        probeSide = null!;
        return false;
    }

    // Whether the expression references at least one slot and all of them come from the
    // given input, so it can be evaluated on that side alone.
    private static bool ReferencesOnly(LogicalExpression expression, ImmutableArray<ValueSlot> slots)
    {
        var referenced = LogicalSlotReferenceFinder.FindReferencedSlots(expression);
        return referenced.Count > 0 && referenced.All(slots.Contains);
    }

    private static bool ReferencesRecursiveReference(LogicalOperator node)
    {
        return node.DescendantsAndSelf().Any(d => d.Kind == LogicalOperatorKind.RecursiveReference);
    }

    private static bool ReferencesAny(LogicalOperator node, ImmutableArray<ValueSlot> slots)
    {
        if (slots.IsEmpty)
            return false;

        var referenced = LogicalSlotReferenceFinder.FindReferencedSlots(node);
        return slots.Any(referenced.Contains);
    }

    // A plain slot reference is the key directly; any other expression is materialized
    // into a fresh slot by a compute the caller places on the input.
    private static ValueSlot AsKeySlot(LogicalExpression expression, out LogicalComputedValue? computedValue)
    {
        if (expression is LogicalValueSlotExpression slot)
        {
            computedValue = null;
            return slot.ValueSlot;
        }

        var referenced = LogicalSlotReferenceFinder.FindReferencedSlots(expression);
        var key = referenced.First().Factory.CreateTemporary(expression.Type);
        computedValue = new LogicalComputedValue(expression, key);
        return key;
    }

    private readonly struct EquiKey
    {
        public EquiKey(ValueSlot buildKey, ValueSlot probeKey, LogicalComputedValue? buildComputedValue, LogicalComputedValue? probeComputedValue, ImmutableArray<LogicalExpression> remainder)
        {
            BuildKey = buildKey;
            ProbeKey = probeKey;
            BuildComputedValue = buildComputedValue;
            ProbeComputedValue = probeComputedValue;
            Remainder = remainder;
        }

        public ValueSlot BuildKey { get; }

        public ValueSlot ProbeKey { get; }

        // The compute that materializes the build/probe key, or null when the key is
        // already a plain slot.
        public LogicalComputedValue? BuildComputedValue { get; }

        public LogicalComputedValue? ProbeComputedValue { get; }

        // The join condition's conjuncts other than the chosen hash-key equality.
        public ImmutableArray<LogicalExpression> Remainder { get; }
    }

    // FULL OUTER JOIN expanded into operators nested loops can run. This is the
    // fallback for a non-equi condition; an equi full outer goes to a hash match
    // above (one pass over each input, no double scan or clone).
    //
    //   (L LEFT OUTER JOIN R ON p)
    //   UNION ALL
    //   (project (NULL-as-L, R) over (R LEFT ANTI SEMI JOIN L ON p))
    //
    // The second branch contributes the right rows with no left match, the left
    // columns padded with NULL. Each branch is an independent scan of L and R, so
    // each gets a slot-disjoint clone of the inputs (and the condition); the union
    // re-defines the join's original output slots by unifying the branches column by
    // column.
    private static LogicalOperator ExpandFullOuterJoin(LogicalJoin node)
    {
        var outputs = node.OutputValueSlots;

        // Branch 1: L LEFT OUTER JOIN R -- outputs L's columns then R's.
        var cloner1 = new LogicalOperatorCloner();
        var branch1 = new LogicalJoin(
            LogicalJoinKind.LeftOuter,
            cloner1.Clone(node.Left),
            cloner1.Clone(node.Right),
            [.. node.Conditions.Select(cloner1.CloneExpression)],
            probe: null,
            passthruPredicate: null);

        // Branch 2: the right rows with no left match, left columns padded with NULL.
        var cloner2 = new LogicalOperatorCloner();
        var right2 = cloner2.Clone(node.Right);
        var left2 = cloner2.Clone(node.Left);
        var antiSemi = new LogicalJoin(
            LogicalJoinKind.LeftAntiSemi,
            right2,
            left2,
            [.. node.Conditions.Select(cloner2.CloneExpression)],
            probe: null,
            passthruPredicate: null);

        var nullSlots = left2.OutputValueSlots.Select(s => s.Duplicate()).ToImmutableArray();
        var nullValues = nullSlots.Select(s => new LogicalComputedValue(new LogicalLiteralExpression(null), s)).ToImmutableArray();
        var compute = new LogicalCompute(antiSemi, nullValues);

        // Reorder to (NULL-as-L ++ R), matching branch 1's (L ++ R) column order.
        var branch2 = new LogicalProject(compute, [.. nullSlots, .. right2.OutputValueSlots]);

        var firstOutputs = branch1.OutputValueSlots;
        var secondOutputs = branch2.OutputValueSlots;
        var unifiedValues = Enumerable.Range(0, outputs.Length)
                                      .Select(i => new LogicalUnifiedValue(outputs[i], new[] { firstOutputs[i], secondOutputs[i] }))
                                      .ToImmutableArray();

        return new LogicalUnion(isUnionAll: true, [branch1, branch2], unifiedValues, []);
    }

    // A full outer join has no PhysicalJoinKind: nested loops can't produce it.
    // PlanNestedLoops expands it (into left-outer UNION ALL right-anti-semi) before
    // mapping, so it shouldn't reach here; this is a defensive guard in case a
    // LogicalJoin with that kind reaches MapJoinKind by another path.
    private static PhysicalJoinKind MapJoinKind(LogicalJoinKind kind)
    {
        return kind switch
        {
            LogicalJoinKind.Inner => PhysicalJoinKind.Inner,
            LogicalJoinKind.LeftOuter => PhysicalJoinKind.LeftOuter,
            LogicalJoinKind.LeftSemi => PhysicalJoinKind.LeftSemi,
            LogicalJoinKind.LeftAntiSemi => PhysicalJoinKind.LeftAntiSemi,
            LogicalJoinKind.FullOuter => throw new UnreachableException("FULL OUTER JOIN has no nested-loops form; PlanNestedLoops expands or hash-matches it before reaching MapJoinKind."),
            _ => throw ExceptionBuilder.UnexpectedValue(kind)
        };
    }

    // An Apply is a dependent join: it has no join condition of its own (the
    // correlation lives in the right subtree) and is executed as nested loops with
    // the referenced left columns exposed to the right as outer references.
    private static PhysicalOperator PlanApply(LogicalApply node, ImmutableArray<ValueSlot> outerSlots)
    {
        var left = PlanOperator(node.Left, outerSlots);

        // The right sees the left's referenced columns on top of the ambient outer
        // scope -- the same accumulation the emitter and verifier perform. This is what
        // lets a correlated filter inside the right be recognized as an index-spool
        // candidate (see PlanFilter).
        var rightOuterSlots = node.OuterReferences.IsEmpty
            ? outerSlots
            : outerSlots.AddRange(node.OuterReferences);
        var right = PlanOperator(node.Right, rightOuterSlots);

        return new PhysicalNestedLoops(MapApplyKind(node.ApplyKind), left, right, [], node.Probe, node.Passthru, node.OuterReferences);
    }

    private static PhysicalJoinKind MapApplyKind(LogicalApplyKind kind)
    {
        return kind switch
        {
            LogicalApplyKind.Inner => PhysicalJoinKind.Inner,
            LogicalApplyKind.LeftOuter => PhysicalJoinKind.LeftOuter,
            LogicalApplyKind.LeftSemi => PhysicalJoinKind.LeftSemi,
            LogicalApplyKind.LeftAntiSemi => PhysicalJoinKind.LeftAntiSemi,
            _ => throw ExceptionBuilder.UnexpectedValue(kind)
        };
    }

    private static PhysicalOperator PlanStreamAggregates(LogicalAggregate node, ImmutableArray<ValueSlot> outerSlots)
    {
        var input = PlanOperator(node.Input, outerSlots);

        // A stream aggregate consumes its input one group at a time, so the rows must
        // arrive grouped. Sort on the grouping columns (reusing their comparers) to
        // guarantee that. A hash aggregate, which wouldn't need this, is future work.
        if (!node.Groups.IsEmpty)
            input = new PhysicalSort(isDistinct: false, input, node.Groups);

        return new PhysicalStreamAggregates(input, node.Groups, node.Aggregates);
    }

    private static PhysicalOperator PlanUnion(LogicalUnion node, ImmutableArray<ValueSlot> outerSlots)
    {
        var inputs = node.Inputs.Select(i => PlanOperator(i, outerSlots)).ToImmutableArray();
        var concatenation = new PhysicalConcatenation(inputs, node.DefinedValues);
        if (node.IsUnionAll)
            return concatenation;

        // A plain UNION removes duplicates. Mirror the lowering: a distinct
        // sort over the unified columns (with their comparers) above the concatenation.
        var sortedValues = node.DefinedValues
                               .Zip(node.Comparers, (v, c) => new LogicalComparedValue(v.ValueSlot, c))
                               .ToImmutableArray();
        return new PhysicalSort(isDistinct: true, concatenation, sortedValues);
    }

    // The recursion node lowers one-to-one; only its subtrees choose algorithms. The
    // recursive reference is an ordinary relation here, so the join planner above is
    // free to pick a hash match for the recursive step (the working-table model's whole
    // point); an index-spool specialization could slot in here later.
    private static PhysicalOperator PlanRecursiveUnion(LogicalRecursiveUnion node, ImmutableArray<ValueSlot> outerSlots)
    {
        var anchor = PlanOperator(node.Anchor, outerSlots);
        var members = node.RecursiveMembers.Select(m => PlanOperator(m, outerSlots)).ToImmutableArray();
        return new PhysicalRecursiveUnion(node.Token, anchor, members, node.DefinedValues);
    }

    private static PhysicalOperator PlanRecursiveReference(LogicalRecursiveReference node)
    {
        return new PhysicalRecursiveReference(node.Token, node.OutputValueSlots);
    }

    private static PhysicalOperator PlanIntersectOrExcept(LogicalIntersectOrExcept node, ImmutableArray<ValueSlot> outerSlots)
    {
        // INTERSECT / EXCEPT lower to a distinct sort on the left feeding a semi-join
        // (intersect) or anti-semi-join (except) against the right, matching on every
        // column with NULLs treated as equal. The intersect-vs-except split is made
        // here, so there is no dedicated physical or executable set-operation node.
        var left = PlanOperator(node.Left, outerSlots);
        var right = PlanOperator(node.Right, outerSlots);

        var leftValues = left.OutputValueSlots;
        var rightValues = right.OutputValueSlots;

        var sortedValues = leftValues.Zip(node.Comparers, (v, c) => new LogicalComparedValue(v, c)).ToImmutableArray();
        var distinctLeft = new PhysicalSort(isDistinct: true, left, sortedValues);

        var conditions = Enumerable.Range(0, leftValues.Length)
                                   .Select(i => BuildNullSafeEquality(leftValues[i], rightValues[i]))
                                   .ToImmutableArray();

        var joinKind = node.IsIntersect ? PhysicalJoinKind.LeftSemi : PhysicalJoinKind.LeftAntiSemi;

        return new PhysicalNestedLoops(joinKind, distinctLeft, right, conditions, probe: null, passthruPredicate: null, []);
    }

    // (l = r) OR (l IS NULL AND r IS NULL). Plain equality yields NULL when either
    // side is NULL, so two NULLs would not match; the second disjunct restores the
    // set-operation rule that NULL equals NULL. A single-NULL pair stays NULL, which
    // the nested-loops predicate compiles to a non-match.
    private static LogicalExpression BuildNullSafeEquality(ValueSlot left, ValueSlot right)
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

    private static PhysicalOperator PlanSort(LogicalSort node, ImmutableArray<ValueSlot> outerSlots)
    {
        var input = PlanOperator(node.Input, outerSlots);
        return new PhysicalSort(node.IsDistinct, input, node.SortedValues);
    }

    private static PhysicalOperator PlanTop(LogicalTop node, ImmutableArray<ValueSlot> outerSlots)
    {
        var input = PlanOperator(node.Input, outerSlots);
        return new PhysicalTop(input, node.Limit, node.TieEntries);
    }

    private static PhysicalOperator PlanAssert(LogicalAssert node, ImmutableArray<ValueSlot> outerSlots)
    {
        var input = PlanOperator(node.Input, outerSlots);
        return new PhysicalAssert(input, node.Condition, node.Message);
    }
}
