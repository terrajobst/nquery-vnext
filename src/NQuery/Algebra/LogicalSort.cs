#nullable enable

using System.Collections.Frozen;
using System.Collections.Immutable;

using NQuery.Binding;

namespace NQuery.Algebra
{
    internal sealed class LogicalSort : LogicalOperator
    {
        public LogicalSort(bool isDistinct, LogicalOperator input, ImmutableArray<BoundComparedValue> sortedValues)
        {
            IsDistinct = isDistinct;
            Input = input;
            SortedValues = sortedValues;
        }

        public override LogicalOperatorKind Kind => LogicalOperatorKind.Sort;

        public bool IsDistinct { get; }

        public LogicalOperator Input { get; }

        public ImmutableArray<BoundComparedValue> SortedValues { get; }

        protected override FrozenSet<ValueSlot> ComputeDefinedValueSlots() => Input.DefinedValueSlots;

        protected override ImmutableArray<ValueSlot> ComputeOutputValueSlots() => Input.OutputValueSlots;
    }
}
