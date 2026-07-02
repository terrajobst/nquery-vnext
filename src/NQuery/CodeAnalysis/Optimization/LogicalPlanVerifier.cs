using System.Collections.Frozen;
using System.Collections.Immutable;

using NQuery.CodeAnalysis.Algebra;

namespace NQuery.CodeAnalysis.Optimization;

// A DEBUG-only structural check on the logical plan, run by LogicalOptimizer after
// every pass that changed the tree (and once on the algebrized input). It walks the
// tree carrying the outer scope an enclosing apply exposes to its right side -- the
// correlation -- and asserts every slot an operator references is available there:
// produced by the operator's own input(s), or supplied by that outer scope.
//
// Running it per pass means a rewrite that drops a still-referenced slot, hoists an
// expression above the operator defining its slot, or mishandles a correlation is
// caught right after the offending pass, with that pass named in the error -- rather
// than surfacing much later as a cryptic slot-lookup failure during emit. The
// physical verifier is the same idea over the post-planning tree (the planner is a
// separate stage this one never sees).
internal static class LogicalPlanVerifier
{
    // source identifies what produced this tree (e.g. the pass name), so the failure
    // points straight at the culprit.
    public static void Verify(LogicalOperator root, string source)
    {
        ThrowIfNull(root);
        VerifyOperator(root, [], source);
        PlanVerification.RequireUniqueDefinitions(source, root, Children, n => n.DefinedValueSlots, n => n.Kind.ToString());
    }

    // The direct child operators of a node, for the slot-uniqueness walk. (The scope
    // walk above recurses into the same children inline, but its per-kind methods also
    // thread the apply scope, so they don't compose into a plain child enumeration.)
    private static ImmutableArray<LogicalOperator> Children(LogicalOperator node) =>
        node switch
        {
            LogicalFilter n => [n.Input],
            LogicalCompute n => [n.Input],
            LogicalProject n => [n.Input],
            LogicalAggregate n => [n.Input],
            LogicalSort n => [n.Input],
            LogicalTop n => [n.Input],
            LogicalAssert n => [n.Input],
            LogicalJoin n => [n.Left, n.Right],
            LogicalApply n => [n.Left, n.Right],
            LogicalIntersectOrExcept n => [n.Left, n.Right],
            LogicalUnion n => n.Inputs,
            LogicalRecursiveUnion n => [n.Anchor, .. n.RecursiveMembers],
            LogicalEmpty or LogicalConstant or LogicalTableScan or LogicalRecursiveReference => [],
            _ => throw ExceptionBuilder.UnexpectedValue(node.Kind),
        };

    private static void VerifyOperator(LogicalOperator node, ImmutableArray<ValueSlot> outerSlots, string source)
    {
        switch (node.Kind)
        {
            case LogicalOperatorKind.Empty:
            case LogicalOperatorKind.Constant:
            case LogicalOperatorKind.TableScan:
            case LogicalOperatorKind.RecursiveReference:
                break;

            case LogicalOperatorKind.Filter:
                VerifyFilter((LogicalFilter)node, outerSlots, source);
                break;
            case LogicalOperatorKind.Compute:
                VerifyCompute((LogicalCompute)node, outerSlots, source);
                break;
            case LogicalOperatorKind.Project:
                VerifyProject((LogicalProject)node, outerSlots, source);
                break;
            case LogicalOperatorKind.Join:
                VerifyJoin((LogicalJoin)node, outerSlots, source);
                break;
            case LogicalOperatorKind.Apply:
                VerifyApply((LogicalApply)node, outerSlots, source);
                break;
            case LogicalOperatorKind.Aggregate:
                VerifyAggregate((LogicalAggregate)node, outerSlots, source);
                break;
            case LogicalOperatorKind.Union:
                VerifyUnion((LogicalUnion)node, outerSlots, source);
                break;
            case LogicalOperatorKind.RecursiveUnion:
                VerifyRecursiveUnion((LogicalRecursiveUnion)node, outerSlots, source);
                break;
            case LogicalOperatorKind.IntersectOrExcept:
                VerifyIntersectOrExcept((LogicalIntersectOrExcept)node, outerSlots, source);
                break;
            case LogicalOperatorKind.Sort:
                VerifySort((LogicalSort)node, outerSlots, source);
                break;
            case LogicalOperatorKind.Top:
                VerifyTop((LogicalTop)node, outerSlots, source);
                break;
            case LogicalOperatorKind.Assert:
                VerifyAssert((LogicalAssert)node, outerSlots, source);
                break;
            default:
                throw ExceptionBuilder.UnexpectedValue(node.Kind);
        }
    }

    private static void VerifyFilter(LogicalFilter node, ImmutableArray<ValueSlot> outerSlots, string source)
    {
        VerifyOperator(node.Input, outerSlots, source);

        var scope = Scope(outerSlots, node.Input.OutputValueSlots);
        foreach (var condition in node.Conditions)
            PlanVerification.Require(source, "Filter", "condition", condition, scope);
    }

    private static void VerifyCompute(LogicalCompute node, ImmutableArray<ValueSlot> outerSlots, string source)
    {
        VerifyOperator(node.Input, outerSlots, source);

        var scope = Scope(outerSlots, node.Input.OutputValueSlots);
        foreach (var value in node.DefinedValues)
            PlanVerification.Require(source, "Compute", "computed value", value.Expression, scope);
    }

    private static void VerifyProject(LogicalProject node, ImmutableArray<ValueSlot> outerSlots, string source)
    {
        VerifyOperator(node.Input, outerSlots, source);

        var scope = Scope(outerSlots, node.Input.OutputValueSlots);
        PlanVerification.Require(source, "Project", "projected slot", node.Outputs, scope);
    }

    private static void VerifyJoin(LogicalJoin node, ImmutableArray<ValueSlot> outerSlots, string source)
    {
        // A logical join's two sides are independent (correlation is modeled as Apply,
        // not Join), so the right does not see the left -- both just see the ambient outer.
        VerifyOperator(node.Left, outerSlots, source);
        VerifyOperator(node.Right, outerSlots, source);

        var scope = Scope(outerSlots, node.Left.OutputValueSlots, node.Right.OutputValueSlots);
        foreach (var condition in node.Conditions)
            PlanVerification.Require(source, "Join", "condition", condition, scope);
        PlanVerification.Require(source, "Join", "passthru predicate", node.PassthruPredicate, scope);
    }

    private static void VerifyApply(LogicalApply node, ImmutableArray<ValueSlot> outerSlots, string source)
    {
        VerifyOperator(node.Left, outerSlots, source);

        // The defining property of an apply: the right is evaluated per left row and may
        // reference the left's columns. So the right sees the left's outputs on top of the
        // ambient outer scope.
        var rightOuter = outerSlots.AddRange(node.Left.OutputValueSlots);
        VerifyOperator(node.Right, rightOuter, source);

        // The passthru guard is a predicate over the left row.
        var leftScope = Scope(outerSlots, node.Left.OutputValueSlots);
        PlanVerification.Require(source, "Apply", "passthru predicate", node.Passthru, leftScope);
    }

    private static void VerifyAggregate(LogicalAggregate node, ImmutableArray<ValueSlot> outerSlots, string source)
    {
        VerifyOperator(node.Input, outerSlots, source);

        var scope = Scope(outerSlots, node.Input.OutputValueSlots);
        PlanVerification.Require(source, "Aggregate", "grouping key", node.Groups.Select(g => g.ValueSlot), scope);
        foreach (var aggregate in node.Aggregates)
            PlanVerification.Require(source, "Aggregate", "aggregate argument", aggregate.Argument, scope);
    }

    private static void VerifyUnion(LogicalUnion node, ImmutableArray<ValueSlot> outerSlots, string source)
    {
        foreach (var input in node.Inputs)
            VerifyOperator(input, outerSlots, source);

        // Each unified output column reads one slot per input, drawn from that input.
        for (var i = 0; i < node.Inputs.Length; i++)
        {
            var scope = Scope(outerSlots, node.Inputs[i].OutputValueSlots);
            PlanVerification.Require(source, "Union", "unified input slot", node.DefinedValues.Select(d => d.InputValueSlots[i]), scope);
        }
    }

    private static void VerifyRecursiveUnion(LogicalRecursiveUnion node, ImmutableArray<ValueSlot> outerSlots, string source)
    {
        VerifyOperator(node.Anchor, outerSlots, source);
        foreach (var member in node.RecursiveMembers)
            VerifyOperator(member, outerSlots, source);

        // Each unified output column reads the anchor's slot and one slot per recursive
        // member, drawn from that input alone (anchor is input 0, members follow).
        var anchorScope = Scope(outerSlots, node.Anchor.OutputValueSlots);
        PlanVerification.Require(source, "RecursiveUnion", "unified anchor slot", node.DefinedValues.Select(d => d.InputValueSlots[0]), anchorScope);

        for (var i = 0; i < node.RecursiveMembers.Length; i++)
        {
            var memberScope = Scope(outerSlots, node.RecursiveMembers[i].OutputValueSlots);
            PlanVerification.Require(source, "RecursiveUnion", "unified member slot", node.DefinedValues.Select(d => d.InputValueSlots[i + 1]), memberScope);
        }
    }

    private static void VerifyIntersectOrExcept(LogicalIntersectOrExcept node, ImmutableArray<ValueSlot> outerSlots, string source)
    {
        // Compares its two inputs positionally; it references no slots of its own.
        VerifyOperator(node.Left, outerSlots, source);
        VerifyOperator(node.Right, outerSlots, source);
    }

    private static void VerifySort(LogicalSort node, ImmutableArray<ValueSlot> outerSlots, string source)
    {
        VerifyOperator(node.Input, outerSlots, source);

        var scope = Scope(outerSlots, node.Input.OutputValueSlots);
        PlanVerification.Require(source, "Sort", "sort key", node.SortedValues.Select(v => v.ValueSlot), scope);
    }

    private static void VerifyTop(LogicalTop node, ImmutableArray<ValueSlot> outerSlots, string source)
    {
        VerifyOperator(node.Input, outerSlots, source);

        var scope = Scope(outerSlots, node.Input.OutputValueSlots);
        PlanVerification.Require(source, "Top", "tie-break key", node.TieEntries.Select(t => t.ValueSlot), scope);
    }

    private static void VerifyAssert(LogicalAssert node, ImmutableArray<ValueSlot> outerSlots, string source)
    {
        VerifyOperator(node.Input, outerSlots, source);

        var scope = Scope(outerSlots, node.Input.OutputValueSlots);
        PlanVerification.Require(source, "Assert", "condition", node.Condition, scope);
    }

    private static FrozenSet<ValueSlot> Scope(ImmutableArray<ValueSlot> outerSlots, params ImmutableArray<ValueSlot>[] inputs)
    {
        var set = new HashSet<ValueSlot>(outerSlots);
        foreach (var input in inputs)
            set.UnionWith(input);
        return set.ToFrozenSet();
    }
}
