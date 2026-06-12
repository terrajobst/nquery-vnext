#nullable enable

using System.Collections.Frozen;
using System.Collections.Immutable;

using NQuery.AlgebraBinding;

namespace NQuery.Algebra
{
    internal sealed class LogicalCompute : LogicalOperator
    {
        public LogicalCompute(LogicalOperator input, ImmutableArray<LogicalComputedValue> definedValues)
        {
            Input = input;
            DefinedValues = definedValues;
        }

        public override LogicalOperatorKind Kind => LogicalOperatorKind.Compute;

        public LogicalOperator Input { get; }

        public ImmutableArray<LogicalComputedValue> DefinedValues { get; }

        protected override FrozenSet<ValueSlot> ComputeDefinedValueSlots() => Input.DefinedValueSlots.Concat(DefinedValues.Select(c => c.ValueSlot)).ToFrozenSet();

        protected override ImmutableArray<ValueSlot> ComputeOutputValueSlots() => Input.OutputValueSlots.Concat(DefinedValues.Select(c => c.ValueSlot)).ToImmutableArray();
    }
}
