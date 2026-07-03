using System.Collections.Frozen;
using System.Collections.Immutable;

using NQuery.CodeAnalysis.Algebra;

namespace NQuery.CodeAnalysis.Planning;

internal sealed class PhysicalFilter : PhysicalOperator
{
    public PhysicalFilter(PhysicalOperator input, ImmutableArray<LogicalExpression> conditions)
    {
        ThrowIfNull(input);

        Input = input;
        Conditions = conditions;
    }

    public override PhysicalOperatorKind Kind => PhysicalOperatorKind.Filter;

    public PhysicalOperator Input { get; }

    public ImmutableArray<LogicalExpression> Conditions { get; }

    protected override FrozenSet<ValueSlot> ComputeDefinedValueSlots() => Input.DefinedValueSlots;

    protected override ImmutableArray<ValueSlot> ComputeOutputValueSlots() => Input.OutputValueSlots;
}
