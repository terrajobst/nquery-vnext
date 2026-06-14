#nullable enable

using System.Collections.Frozen;
using System.Collections.Immutable;

using NQuery.Algebra;

namespace NQuery.Planning;

internal sealed class PhysicalSort : PhysicalOperator
{
    public PhysicalSort(bool isDistinct, PhysicalOperator input, ImmutableArray<LogicalComparedValue> sortedValues)
    {
        IsDistinct = isDistinct;
        Input = input;
        SortedValues = sortedValues;
    }

    public override PhysicalOperatorKind Kind => PhysicalOperatorKind.Sort;

    public bool IsDistinct { get; }

    public PhysicalOperator Input { get; }

    public ImmutableArray<LogicalComparedValue> SortedValues { get; }

    protected override FrozenSet<ValueSlot> ComputeDefinedValueSlots() => Input.DefinedValueSlots;

    protected override ImmutableArray<ValueSlot> ComputeOutputValueSlots() => Input.OutputValueSlots;
}
