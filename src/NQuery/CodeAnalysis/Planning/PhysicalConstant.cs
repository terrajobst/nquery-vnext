using System.Collections.Frozen;
using System.Collections.Immutable;

using NQuery.CodeAnalysis.Algebra;

namespace NQuery.CodeAnalysis.Planning;

internal sealed class PhysicalConstant : PhysicalOperator
{
    public override PhysicalOperatorKind Kind => PhysicalOperatorKind.Constant;

    protected override FrozenSet<ValueSlot> ComputeDefinedValueSlots() => [];

    protected override ImmutableArray<ValueSlot> ComputeOutputValueSlots() => [];
}
