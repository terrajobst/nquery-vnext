#nullable enable

using System.Collections.Frozen;
using System.Collections.Immutable;

using NQuery.Algebra;
using NQuery.Binding;

namespace NQuery.Planning
{
    // A single physical aggregate for now. Choosing between a stream aggregate
    // (over sorted input) and a hash aggregate is a later planning refinement.
    internal sealed class PhysicalAggregate : PhysicalOperator
    {
        public PhysicalAggregate(PhysicalOperator input, ImmutableArray<BoundComparedValue> groups, ImmutableArray<LogicalAggregatedValue> aggregates)
        {
            Input = input;
            Groups = groups;
            Aggregates = aggregates;
        }

        public override PhysicalOperatorKind Kind => PhysicalOperatorKind.Aggregate;

        public PhysicalOperator Input { get; }

        public ImmutableArray<BoundComparedValue> Groups { get; }

        public ImmutableArray<LogicalAggregatedValue> Aggregates { get; }

        protected override FrozenSet<ValueSlot> ComputeDefinedValueSlots() => Groups.Select(g => g.ValueSlot).Concat(Aggregates.Select(a => a.Output)).ToFrozenSet();

        protected override ImmutableArray<ValueSlot> ComputeOutputValueSlots() => Groups.Select(g => g.ValueSlot).Concat(Aggregates.Select(a => a.Output)).ToImmutableArray();
    }
}
