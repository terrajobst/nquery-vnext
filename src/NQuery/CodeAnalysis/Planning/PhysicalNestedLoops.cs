using System.Collections.Frozen;
using System.Collections.Immutable;

using NQuery.CodeAnalysis.Algebra;

namespace NQuery.CodeAnalysis.Planning;

// The nested-loops realization of a logical join: for each outer (left) row it
// rescans the inner (right). It is the general-purpose join algorithm -- it works
// for any predicate. Equi-joins will get a sibling PhysicalHashMatch once that is
// built; until then every join is planned as nested loops. Slot flow is identical
// to the logical join.
//
// It is also the physical form of an Apply (a dependent join): OuterReferences are
// the left columns the right side reads, so the executor threads those values into
// the right's evaluation. A plain join has no outer references; an apply has the
// referenced left slots, no Conditions (the correlation lives in the right's own
// filters), and its ApplyKind mapped onto JoinKind.
internal sealed class PhysicalNestedLoops : PhysicalOperator
{
    public PhysicalNestedLoops(PhysicalJoinKind joinKind, PhysicalOperator left, PhysicalOperator right, ImmutableArray<LogicalExpression> conditions, ValueSlot? probe, LogicalExpression? passthruPredicate, ImmutableArray<ValueSlot> outerReferences)
    {
        JoinKind = joinKind;
        Left = left;
        Right = right;
        Conditions = conditions;
        Probe = probe;
        PassthruPredicate = passthruPredicate;
        OuterReferences = outerReferences;
    }

    public override PhysicalOperatorKind Kind => PhysicalOperatorKind.NestedLoops;

    public PhysicalJoinKind JoinKind { get; }

    public PhysicalOperator Left { get; }

    public PhysicalOperator Right { get; }

    public ImmutableArray<LogicalExpression> Conditions { get; }

    public ValueSlot? Probe { get; }

    public LogicalExpression? PassthruPredicate { get; }

    // The left columns the right side references (an Apply's correlation). Empty
    // for an ordinary, independent join. When non-empty, the right is re-evaluated
    // per left row with these values in scope.
    public ImmutableArray<ValueSlot> OuterReferences { get; }

    protected override FrozenSet<ValueSlot> ComputeDefinedValueSlots()
    {
        var result = Left.DefinedValueSlots.Concat(Right.DefinedValueSlots);
        if (Probe is not null)
            result = result.Append(Probe);
        return result.ToFrozenSet();
    }

    protected override ImmutableArray<ValueSlot> ComputeOutputValueSlots()
    {
        IEnumerable<ValueSlot> result = Left.OutputValueSlots;
        if (IncludeRightValues())
            result = result.Concat(Right.OutputValueSlots);
        if (Probe is not null)
            result = result.Append(Probe);
        return result.ToImmutableArray();
    }

    private bool IncludeRightValues()
    {
        return JoinKind switch
        {
            PhysicalJoinKind.Inner or PhysicalJoinKind.LeftOuter => true,
            PhysicalJoinKind.LeftSemi or PhysicalJoinKind.LeftAntiSemi => false,
            _ => throw ExceptionBuilder.UnexpectedValue(JoinKind)
        };
    }
}
