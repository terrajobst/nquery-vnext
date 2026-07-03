using System.Collections.Frozen;
using System.Collections.Immutable;

namespace NQuery.CodeAnalysis.Algebra;

internal sealed class LogicalSort : LogicalOperator
{
    public LogicalSort(bool isDistinct, LogicalOperator input, ImmutableArray<LogicalComparedValue> sortedValues)
    {
        ThrowIfNull(input);

        IsDistinct = isDistinct;
        Input = input;
        SortedValues = sortedValues;
    }

    public override LogicalOperatorKind Kind => LogicalOperatorKind.Sort;

    public bool IsDistinct { get; }

    public LogicalOperator Input { get; }

    public ImmutableArray<LogicalComparedValue> SortedValues { get; }

    protected override FrozenSet<ValueSlot> ComputeDefinedValueSlots() => Input.DefinedValueSlots;

    protected override ImmutableArray<ValueSlot> ComputeOutputValueSlots() => Input.OutputValueSlots;
}
