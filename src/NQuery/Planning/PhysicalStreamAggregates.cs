#nullable enable

using System.Collections.Frozen;
using System.Collections.Immutable;

using NQuery.Refactor.Algebra;
using NQuery.Refactor.Binding;

namespace NQuery.Planning
{
    // Aggregation as a stream aggregate: it consumes input the planner has sorted on
    // the grouping columns and collapses each run of equal-group rows. Choosing a hash
    // aggregate instead is a later planning refinement.
    internal sealed class PhysicalStreamAggregates : PhysicalOperator
    {
        public PhysicalStreamAggregates(PhysicalOperator input, ImmutableArray<LogicalComparedValue> groups, ImmutableArray<LogicalAggregatedValue> aggregates)
        {
            Input = input;
            Groups = groups;
            Aggregates = aggregates;
        }

        public override PhysicalOperatorKind Kind => PhysicalOperatorKind.StreamAggregates;

        public PhysicalOperator Input { get; }

        public ImmutableArray<LogicalComparedValue> Groups { get; }

        public ImmutableArray<LogicalAggregatedValue> Aggregates { get; }

        protected override FrozenSet<ValueSlot> ComputeDefinedValueSlots() => Groups.Select(g => g.ValueSlot).Concat(Aggregates.Select(a => a.Output)).ToFrozenSet();

        protected override ImmutableArray<ValueSlot> ComputeOutputValueSlots() => Groups.Select(g => g.ValueSlot).Concat(Aggregates.Select(a => a.Output)).ToImmutableArray();
    }
}
