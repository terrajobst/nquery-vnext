#nullable enable

using System.Collections.Frozen;
using System.Collections.Immutable;

using NQuery.AlgebraBinding;

namespace NQuery.Planning
{
    internal sealed class PhysicalProject : PhysicalOperator
    {
        public PhysicalProject(PhysicalOperator input, ImmutableArray<ValueSlot> outputs)
        {
            Input = input;
            Outputs = outputs;
        }

        public override PhysicalOperatorKind Kind => PhysicalOperatorKind.Project;

        public PhysicalOperator Input { get; }

        public ImmutableArray<ValueSlot> Outputs { get; }

        protected override FrozenSet<ValueSlot> ComputeDefinedValueSlots() => Input.DefinedValueSlots;

        protected override ImmutableArray<ValueSlot> ComputeOutputValueSlots() => Outputs;
    }
}
