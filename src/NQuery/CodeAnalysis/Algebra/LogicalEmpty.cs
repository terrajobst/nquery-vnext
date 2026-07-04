using System.Collections.Frozen;
using System.Collections.Immutable;

namespace NQuery.CodeAnalysis.Algebra;

internal sealed class LogicalEmpty : LogicalOperator
{
    public override LogicalOperatorKind Kind => LogicalOperatorKind.Empty;

    protected override FrozenSet<ValueSlot> ComputeDefinedValueSlots() => [];

    protected override ImmutableArray<ValueSlot> ComputeOutputValueSlots() => [];
}
