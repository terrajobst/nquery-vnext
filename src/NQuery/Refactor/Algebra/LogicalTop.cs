#nullable enable

using System.Collections.Frozen;
using System.Collections.Immutable;

using NQuery.Refactor.Binding;

namespace NQuery.Refactor.Algebra
{
    internal sealed class LogicalTop : LogicalOperator
    {
        public LogicalTop(LogicalOperator input, int limit, ImmutableArray<LogicalComparedValue> tieEntries)
        {
            Input = input;
            Limit = limit;
            TieEntries = tieEntries;
        }

        public override LogicalOperatorKind Kind => LogicalOperatorKind.Top;

        public LogicalOperator Input { get; }

        public int Limit { get; }

        public ImmutableArray<LogicalComparedValue> TieEntries { get; }

        protected override FrozenSet<ValueSlot> ComputeDefinedValueSlots() => Input.DefinedValueSlots;

        protected override ImmutableArray<ValueSlot> ComputeOutputValueSlots() => Input.OutputValueSlots;
    }
}
