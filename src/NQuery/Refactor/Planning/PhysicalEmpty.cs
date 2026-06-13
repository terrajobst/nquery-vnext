#nullable enable

using System.Collections.Frozen;
using System.Collections.Immutable;

using NQuery.Refactor.Binding;

namespace NQuery.Refactor.Planning
{
    internal sealed class PhysicalEmpty : PhysicalOperator
    {
        public override PhysicalOperatorKind Kind => PhysicalOperatorKind.Empty;

        protected override FrozenSet<ValueSlot> ComputeDefinedValueSlots() => FrozenSet<ValueSlot>.Empty;

        protected override ImmutableArray<ValueSlot> ComputeOutputValueSlots() => ImmutableArray<ValueSlot>.Empty;
    }
}
