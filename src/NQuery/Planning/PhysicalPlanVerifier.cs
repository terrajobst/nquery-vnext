#nullable enable

using System.Collections.Immutable;

using NQuery.Algebra;

namespace NQuery.Planning;

// A debug-time structural check on the physical plan, run between Plan and Emit. It
// walks the tree carrying the same outer-reference scope the emitter threads (its
// outerSlots), and asserts that every value slot an operator's predicates, keys,
// computes, or projections reference is *available* -- i.e. in the union of its
// inputs' output slots and the outer scope an enclosing Apply provides.
//
// A violation means the emitter would later be unable to map that slot to a
// row-buffer position. Before this check that surfaced as a KeyNotFoundException
// deep inside expression compilation (e.g. a decorrelated hash-match remainder that
// referenced an outer column the hash match had dropped from scope), with a stack
// trace pointing at the compiler rather than the offending operator. Catching it
// here names the operator and the slot.
//
// The verifier deliberately mirrors the emitter's scope rules operator by operator,
// so it is a faithful pre-image of the emitter's slot resolution: whatever it
// accepts, the emitter can compile. It is compiled out of release builds -- see the
// #if DEBUG call site in Compilation.Compile.
internal static class PhysicalPlanVerifier
{
    private const string Source = "Physical plan verification failed (after planning)";

    // The production call site (Compilation.Compile) is guarded by #if DEBUG, so this
    // costs nothing in shipping builds; the method itself stays callable so it can be
    // unit-tested directly in any configuration.
    public static void Verify(PhysicalQuery query)
    {
        ThrowIfNull(query);
        VerifyOperator(query.Root, ImmutableArray<ValueSlot>.Empty);
    }

    // outerSlots is the ambient scope an enclosing Apply makes available -- exactly
    // what the emitter prepends to the row buffer. It accumulates as we descend into
    // the right side of a dependent (nested-loops) join.
    private static void VerifyOperator(PhysicalOperator node, ImmutableArray<ValueSlot> outerSlots)
    {
        switch (node.Kind)
        {
            case PhysicalOperatorKind.Empty:
            case PhysicalOperatorKind.Constant:
            case PhysicalOperatorKind.TableScan:
                break;

            case PhysicalOperatorKind.Filter:
                VerifyFilter((PhysicalFilter)node, outerSlots);
                break;
            case PhysicalOperatorKind.ComputeScalar:
                VerifyComputeScalar((PhysicalComputeScalar)node, outerSlots);
                break;
            case PhysicalOperatorKind.Project:
                VerifyProject((PhysicalProject)node, outerSlots);
                break;
            case PhysicalOperatorKind.Sort:
                VerifySort((PhysicalSort)node, outerSlots);
                break;
            case PhysicalOperatorKind.Top:
                VerifyTop((PhysicalTop)node, outerSlots);
                break;
            case PhysicalOperatorKind.StreamAggregates:
                VerifyStreamAggregates((PhysicalStreamAggregates)node, outerSlots);
                break;
            case PhysicalOperatorKind.Assert:
                VerifyAssert((PhysicalAssert)node, outerSlots);
                break;
            case PhysicalOperatorKind.Concatenation:
                VerifyConcatenation((PhysicalConcatenation)node, outerSlots);
                break;
            case PhysicalOperatorKind.NestedLoops:
                VerifyNestedLoops((PhysicalNestedLoops)node, outerSlots);
                break;
            case PhysicalOperatorKind.HashMatch:
                VerifyHashMatch((PhysicalHashMatch)node, outerSlots);
                break;
            default:
                throw ExceptionBuilder.UnexpectedValue(node.Kind);
        }
    }

    private static void VerifyFilter(PhysicalFilter node, ImmutableArray<ValueSlot> outerSlots)
    {
        VerifyOperator(node.Input, outerSlots);

        var scope = Scope(outerSlots, node.Input.OutputValueSlots);
        foreach (var condition in node.Conditions)
            RequireReferences(node, "condition", condition, scope);
    }

    private static void VerifyComputeScalar(PhysicalComputeScalar node, ImmutableArray<ValueSlot> outerSlots)
    {
        VerifyOperator(node.Input, outerSlots);

        var scope = Scope(outerSlots, node.Input.OutputValueSlots);
        foreach (var value in node.DefinedValues)
            RequireReferences(node, "computed value", value.Expression, scope);
    }

    private static void VerifyProject(PhysicalProject node, ImmutableArray<ValueSlot> outerSlots)
    {
        VerifyOperator(node.Input, outerSlots);

        // A project reorders its input's row buffer; its outputs come from the input
        // alone (the emitter's allocation does not see the outer scope).
        var scope = Scope(ImmutableArray<ValueSlot>.Empty, node.Input.OutputValueSlots);
        RequireAvailable(node, "projected slot", node.Outputs, scope);
    }

    private static void VerifySort(PhysicalSort node, ImmutableArray<ValueSlot> outerSlots)
    {
        VerifyOperator(node.Input, outerSlots);

        var scope = Scope(ImmutableArray<ValueSlot>.Empty, node.Input.OutputValueSlots);
        RequireAvailable(node, "sort key", node.SortedValues.Select(v => v.ValueSlot), scope);
    }

    private static void VerifyTop(PhysicalTop node, ImmutableArray<ValueSlot> outerSlots)
    {
        VerifyOperator(node.Input, outerSlots);

        var scope = Scope(ImmutableArray<ValueSlot>.Empty, node.Input.OutputValueSlots);
        RequireAvailable(node, "tie-break key", node.TieEntries.Select(t => t.ValueSlot), scope);
    }

    private static void VerifyStreamAggregates(PhysicalStreamAggregates node, ImmutableArray<ValueSlot> outerSlots)
    {
        VerifyOperator(node.Input, outerSlots);

        var scope = Scope(outerSlots, node.Input.OutputValueSlots);
        RequireAvailable(node, "grouping key", node.Groups.Select(g => g.ValueSlot), scope);
        foreach (var aggregate in node.Aggregates)
            RequireReferences(node, "aggregate argument", aggregate.Argument, scope);
    }

    private static void VerifyAssert(PhysicalAssert node, ImmutableArray<ValueSlot> outerSlots)
    {
        VerifyOperator(node.Input, outerSlots);

        var scope = Scope(outerSlots, node.Input.OutputValueSlots);
        RequireReferences(node, "assert condition", node.Condition, scope);
    }

    private static void VerifyConcatenation(PhysicalConcatenation node, ImmutableArray<ValueSlot> outerSlots)
    {
        foreach (var input in node.Inputs)
            VerifyOperator(input, outerSlots);

        // Each unified output reads one slot per input, drawn from that input alone.
        for (var i = 0; i < node.Inputs.Length; i++)
        {
            var scope = Scope(ImmutableArray<ValueSlot>.Empty, node.Inputs[i].OutputValueSlots);
            RequireAvailable(node, "unified input slot", node.DefinedValues.Select(d => d.InputValueSlots[i]), scope);
        }
    }

    private static void VerifyNestedLoops(PhysicalNestedLoops node, ImmutableArray<ValueSlot> outerSlots)
    {
        VerifyOperator(node.Left, outerSlots);

        // A dependent join exposes its outer references (left columns) to the right,
        // on top of any outer this node itself sits under -- exactly as the emitter
        // accumulates them.
        var rightOuter = node.OuterReferences.IsEmpty
            ? outerSlots
            : outerSlots.AddRange(node.OuterReferences);
        VerifyOperator(node.Right, rightOuter);

        // The outer references are projected from the left's row, so they must be among
        // the left's outputs.
        RequireAvailable(node, "outer reference", node.OuterReferences, Scope(ImmutableArray<ValueSlot>.Empty, node.Left.OutputValueSlots));

        // The join predicate sees both sides plus the ambient outer scope.
        var scope = Scope(outerSlots, node.Left.OutputValueSlots, node.Right.OutputValueSlots);
        foreach (var condition in node.Conditions)
            RequireReferences(node, "condition", condition, scope);
        if (node.PassthruPredicate is not null)
            RequireReferences(node, "passthru predicate", node.PassthruPredicate, scope);
    }

    private static void VerifyHashMatch(PhysicalHashMatch node, ImmutableArray<ValueSlot> outerSlots)
    {
        VerifyOperator(node.Build, outerSlots);
        VerifyOperator(node.Probe, outerSlots);

        RequireAvailable(node, "build key", new[] { node.BuildKey }, Scope(ImmutableArray<ValueSlot>.Empty, node.Build.OutputValueSlots));
        RequireAvailable(node, "probe key", new[] { node.ProbeKey }, Scope(ImmutableArray<ValueSlot>.Empty, node.Probe.OutputValueSlots));

        // The remainder spans both inputs and -- for a correlated hash match inside an
        // apply -- the ambient outer scope.
        var scope = Scope(outerSlots, node.Build.OutputValueSlots, node.Probe.OutputValueSlots);
        foreach (var condition in node.Remainder)
            RequireReferences(node, "remainder", condition, scope);
    }

    private static HashSet<ValueSlot> Scope(ImmutableArray<ValueSlot> outerSlots, params ImmutableArray<ValueSlot>[] inputs)
    {
        var set = new HashSet<ValueSlot>(outerSlots);
        foreach (var input in inputs)
            set.UnionWith(input);
        return set;
    }

    private static void RequireReferences(PhysicalOperator node, string role, LogicalExpression expression, HashSet<ValueSlot> available)
    {
        PlanVerification.Require(Source, node.Kind.ToString(), role, expression, available);
    }

    private static void RequireAvailable(PhysicalOperator node, string role, IEnumerable<ValueSlot> referenced, HashSet<ValueSlot> available)
    {
        PlanVerification.Require(Source, node.Kind.ToString(), role, referenced, available);
    }
}
