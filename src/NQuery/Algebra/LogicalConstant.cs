#nullable enable

using System.Collections.Frozen;
using System.Collections.Immutable;

namespace NQuery.Algebra
{
    internal sealed class LogicalConstant : LogicalOperator
    {
        public override LogicalOperatorKind Kind => LogicalOperatorKind.Constant;

        protected override FrozenSet<ValueSlot> ComputeDefinedValueSlots() => FrozenSet<ValueSlot>.Empty;

        protected override ImmutableArray<ValueSlot> ComputeOutputValueSlots() => ImmutableArray<ValueSlot>.Empty;
    }
}
