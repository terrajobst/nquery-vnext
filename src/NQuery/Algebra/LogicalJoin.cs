using System.Collections.Frozen;
using System.Collections.Immutable;

namespace NQuery.Algebra;

internal sealed class LogicalJoin : LogicalOperator
{
    public LogicalJoin(LogicalJoinKind joinKind, LogicalOperator left, LogicalOperator right, ImmutableArray<LogicalExpression> conditions, ValueSlot? probe, LogicalExpression? passthruPredicate)
    {
        JoinKind = joinKind;
        Left = left;
        Right = right;
        Conditions = conditions;
        Probe = probe;
        PassthruPredicate = passthruPredicate;
    }

    public override LogicalOperatorKind Kind => LogicalOperatorKind.Join;

    public LogicalJoinKind JoinKind { get; }

    public LogicalOperator Left { get; }

    public LogicalOperator Right { get; }

    // Implicit conjunction (AND of all); an empty list means no join predicate.
    public ImmutableArray<LogicalExpression> Conditions { get; }

    public ValueSlot? Probe { get; }

    public LogicalExpression? PassthruPredicate { get; }

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
        switch (JoinKind)
        {
            case LogicalJoinKind.Inner:
            case LogicalJoinKind.FullOuter:
            case LogicalJoinKind.LeftOuter:
                return true;
            case LogicalJoinKind.LeftSemi:
            case LogicalJoinKind.LeftAntiSemi:
                return false;
            default:
                throw ExceptionBuilder.UnexpectedValue(JoinKind);
        }
    }
}
