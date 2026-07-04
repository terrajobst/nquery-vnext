using System.Collections.Frozen;
using System.Collections.Immutable;

namespace NQuery.CodeAnalysis.Algebra;

internal sealed class LogicalConstant : LogicalOperator
{
    public override LogicalOperatorKind Kind => LogicalOperatorKind.Constant;

    protected override FrozenSet<ValueSlot> ComputeDefinedValueSlots() => [];

    protected override ImmutableArray<ValueSlot> ComputeOutputValueSlots() => [];
}
