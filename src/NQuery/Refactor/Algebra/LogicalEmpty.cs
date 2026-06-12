#nullable enable

using System.Collections.Frozen;
using System.Collections.Immutable;

using NQuery.Refactor.Binding;

namespace NQuery.Refactor.Algebra
{
    internal sealed class LogicalEmpty : LogicalOperator
    {
        public override LogicalOperatorKind Kind => LogicalOperatorKind.Empty;

        protected override FrozenSet<ValueSlot> ComputeDefinedValueSlots() => FrozenSet<ValueSlot>.Empty;

        protected override ImmutableArray<ValueSlot> ComputeOutputValueSlots() => ImmutableArray<ValueSlot>.Empty;
    }
}
