using System.Collections.Frozen;
using System.Collections.Immutable;

namespace NQuery.CodeAnalysis.Algebra;

// Logical grouping + aggregation. The choice of a physical implementation
// (stream vs hash aggregate) is a planning concern, not represented here.
internal sealed class LogicalAggregate : LogicalOperator
{
    public LogicalAggregate(LogicalOperator input, ImmutableArray<LogicalComparedValue> groups, ImmutableArray<LogicalAggregatedValue> aggregates)
    {
        Input = input;
        Groups = groups;
        Aggregates = aggregates;
    }

    public override LogicalOperatorKind Kind => LogicalOperatorKind.Aggregate;

    public LogicalOperator Input { get; }

    public ImmutableArray<LogicalComparedValue> Groups { get; }

    public ImmutableArray<LogicalAggregatedValue> Aggregates { get; }

    protected override FrozenSet<ValueSlot> ComputeDefinedValueSlots() => Groups.Select(g => g.ValueSlot).Concat(Aggregates.Select(a => a.Output)).ToFrozenSet();

    protected override ImmutableArray<ValueSlot> ComputeOutputValueSlots() => [.. Groups.Select(g => g.ValueSlot), .. Aggregates.Select(a => a.Output)];
}
