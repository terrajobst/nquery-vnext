#nullable enable

using System.Collections.Frozen;
using System.Collections.Immutable;

using NQuery.Algebra;
using NQuery.AlgebraBinding;

namespace NQuery.Planning
{
    // Passes its input through unchanged, raising an error on any row that fails the
    // condition. It guards a scalar subquery's at-most-one-row rule (the condition is
    // count <= 1 over the aggregated subquery).
    internal sealed class PhysicalAssert : PhysicalOperator
    {
        public PhysicalAssert(PhysicalOperator input, LogicalExpression condition, string message)
        {
            Input = input;
            Condition = condition;
            Message = message;
        }

        public override PhysicalOperatorKind Kind => PhysicalOperatorKind.Assert;

        public PhysicalOperator Input { get; }

        public LogicalExpression Condition { get; }

        public string Message { get; }

        protected override FrozenSet<ValueSlot> ComputeDefinedValueSlots() => Input.DefinedValueSlots;

        protected override ImmutableArray<ValueSlot> ComputeOutputValueSlots() => Input.OutputValueSlots;
    }
}
