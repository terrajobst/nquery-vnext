using System.Collections;
using System.Collections.Frozen;
using System.Collections.Immutable;

namespace NQuery.Algebra;

internal sealed class LogicalIntersectOrExcept : LogicalOperator
{
    public LogicalIntersectOrExcept(bool isIntersect, LogicalOperator left, LogicalOperator right, ImmutableArray<IComparer> comparers)
    {
        IsIntersect = isIntersect;
        Left = left;
        Right = right;
        Comparers = comparers;
    }

    public override LogicalOperatorKind Kind => LogicalOperatorKind.IntersectOrExcept;

    public bool IsIntersect { get; }

    public LogicalOperator Left { get; }

    public LogicalOperator Right { get; }

    public ImmutableArray<IComparer> Comparers { get; }

    protected override FrozenSet<ValueSlot> ComputeDefinedValueSlots() => Left.OutputValueSlots.ToFrozenSet();

    protected override ImmutableArray<ValueSlot> ComputeOutputValueSlots() => Left.OutputValueSlots;
}
