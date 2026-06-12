#nullable enable

using System.Collections.Frozen;
using System.Collections.Immutable;

using NQuery.Refactor.Binding;

namespace NQuery.Planning
{
    internal sealed class PhysicalTop : PhysicalOperator
    {
        public PhysicalTop(PhysicalOperator input, int limit, ImmutableArray<BoundComparedValue> tieEntries)
        {
            Input = input;
            Limit = limit;
            TieEntries = tieEntries;
        }

        public override PhysicalOperatorKind Kind => PhysicalOperatorKind.Top;

        public PhysicalOperator Input { get; }

        public int Limit { get; }

        public ImmutableArray<BoundComparedValue> TieEntries { get; }

        protected override FrozenSet<ValueSlot> ComputeDefinedValueSlots() => Input.DefinedValueSlots;

        protected override ImmutableArray<ValueSlot> ComputeOutputValueSlots() => Input.OutputValueSlots;
    }
}
