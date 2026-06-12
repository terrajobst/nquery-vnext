#nullable enable

using System.Collections.Frozen;
using System.Collections.Immutable;

using NQuery.Refactor.Algebra;
using NQuery.Refactor.Binding;

namespace NQuery.Planning
{
    internal sealed class PhysicalComputeScalar : PhysicalOperator
    {
        public PhysicalComputeScalar(PhysicalOperator input, ImmutableArray<LogicalComputedValue> definedValues)
        {
            Input = input;
            DefinedValues = definedValues;
        }

        public override PhysicalOperatorKind Kind => PhysicalOperatorKind.ComputeScalar;

        public PhysicalOperator Input { get; }

        public ImmutableArray<LogicalComputedValue> DefinedValues { get; }

        protected override FrozenSet<ValueSlot> ComputeDefinedValueSlots() => Input.DefinedValueSlots.Concat(DefinedValues.Select(c => c.ValueSlot)).ToFrozenSet();

        protected override ImmutableArray<ValueSlot> ComputeOutputValueSlots() => Input.OutputValueSlots.Concat(DefinedValues.Select(c => c.ValueSlot)).ToImmutableArray();
    }
}
