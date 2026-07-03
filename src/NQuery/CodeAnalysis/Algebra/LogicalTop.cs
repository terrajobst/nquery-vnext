using System.Collections.Frozen;
using System.Collections.Immutable;

namespace NQuery.CodeAnalysis.Algebra;

internal sealed class LogicalTop : LogicalOperator
{
    public LogicalTop(LogicalOperator input, int limit, ImmutableArray<LogicalComparedValue> tieEntries)
    {
        ThrowIfNull(input);

        Input = input;
        Limit = limit;
        TieEntries = tieEntries;
    }

    public override LogicalOperatorKind Kind => LogicalOperatorKind.Top;

    public LogicalOperator Input { get; }

    public int Limit { get; }

    public ImmutableArray<LogicalComparedValue> TieEntries { get; }

    protected override FrozenSet<ValueSlot> ComputeDefinedValueSlots() => Input.DefinedValueSlots;

    protected override ImmutableArray<ValueSlot> ComputeOutputValueSlots() => Input.OutputValueSlots;
}
