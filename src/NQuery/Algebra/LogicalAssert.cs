#nullable enable

using System.Collections.Frozen;
using System.Collections.Immutable;

using NQuery.Binding;

namespace NQuery.Algebra
{
    internal sealed class LogicalAssert : LogicalOperator
    {
        public LogicalAssert(LogicalOperator input, LogicalExpression condition, string message)
        {
            Input = input;
            Condition = condition;
            Message = message;
        }

        public override LogicalOperatorKind Kind => LogicalOperatorKind.Assert;

        public LogicalOperator Input { get; }

        public LogicalExpression Condition { get; }

        public string Message { get; }

        protected override FrozenSet<ValueSlot> ComputeDefinedValueSlots() => Input.DefinedValueSlots;

        protected override ImmutableArray<ValueSlot> ComputeOutputValueSlots() => Input.OutputValueSlots;
    }
}
