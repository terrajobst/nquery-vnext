#nullable enable

using System.Collections.Frozen;
using System.Collections.Immutable;

using NQuery.Refactor.Algebra;
using NQuery.Refactor.Binding;

namespace NQuery.Refactor.Planning
{
    internal sealed class PhysicalFilter : PhysicalOperator
    {
        public PhysicalFilter(PhysicalOperator input, ImmutableArray<LogicalExpression> conditions)
        {
            Input = input;
            Conditions = conditions;
        }

        public override PhysicalOperatorKind Kind => PhysicalOperatorKind.Filter;

        public PhysicalOperator Input { get; }

        public ImmutableArray<LogicalExpression> Conditions { get; }

        protected override FrozenSet<ValueSlot> ComputeDefinedValueSlots() => Input.DefinedValueSlots;

        protected override ImmutableArray<ValueSlot> ComputeOutputValueSlots() => Input.OutputValueSlots;
    }
}
