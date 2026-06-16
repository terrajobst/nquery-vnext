using System.Collections.Frozen;
using System.Collections.Immutable;

namespace NQuery.CodeAnalysis.Algebra;

internal sealed class LogicalProject : LogicalOperator
{
    public LogicalProject(LogicalOperator input, ImmutableArray<ValueSlot> outputs)
    {
        Input = input;
        Outputs = outputs;
    }

    public override LogicalOperatorKind Kind => LogicalOperatorKind.Project;

    public LogicalOperator Input { get; }

    public ImmutableArray<ValueSlot> Outputs { get; }

    protected override FrozenSet<ValueSlot> ComputeDefinedValueSlots() => Input.DefinedValueSlots;

    protected override ImmutableArray<ValueSlot> ComputeOutputValueSlots() => Outputs;
}
