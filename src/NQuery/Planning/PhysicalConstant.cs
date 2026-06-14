#nullable enable

using System.Collections.Frozen;
using System.Collections.Immutable;

using NQuery.Algebra;

namespace NQuery.Planning
{
    internal sealed class PhysicalConstant : PhysicalOperator
    {
        public override PhysicalOperatorKind Kind => PhysicalOperatorKind.Constant;

        protected override FrozenSet<ValueSlot> ComputeDefinedValueSlots() => FrozenSet<ValueSlot>.Empty;

        protected override ImmutableArray<ValueSlot> ComputeOutputValueSlots() => ImmutableArray<ValueSlot>.Empty;
    }
}
