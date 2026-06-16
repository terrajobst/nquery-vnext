using System.Collections;
using System.Collections.Immutable;

using NQuery.CodeAnalysis.Algebra;
using NQuery.CodeAnalysis.Iterators;

namespace NQuery.CodeAnalysis.Emit;

// Grouping + aggregation, executed as a stream aggregate over input the planner has
// sorted on the grouping columns. The argument expressions, grouping-column positions, and
// the aggregate folds are all compiled once here; the aggregators -- which hold per-run state
// -- are created fresh in CreateIterator. Choosing a hash aggregate instead is a later
// refinement.
internal sealed class ExecutableStreamAggregates : ExecutableOperator
{
    private readonly ExecutableOperator _input;
    private readonly ImmutableArray<int> _groupIndices;
    private readonly ImmutableArray<IComparer> _comparers;
    private readonly Func<EmittedAggregates> _aggregatesFactory;
    private readonly int _aggregateCount;

    public ExecutableStreamAggregates(ImmutableArray<ValueSlot> outputValueSlots, ExecutableOperator input, ImmutableArray<LogicalComparedValue> groups, ImmutableArray<LogicalAggregatedValue> aggregates, ImmutableArray<ValueSlot> outerSlots)
        : base(outputValueSlots)
    {
        _input = input;

        // Compile against (outer ++ input) when correlated, matching the buffer the
        // iterator feeds; otherwise just the input's own slots.
        var slots = outerSlots.IsEmpty ? input.OutputValueSlots : outerSlots.AddRange(input.OutputValueSlots);
        var slotIndices = EmittedExpressionCompiler.CreateSlotIndices(slots);

        _groupIndices = groups.Select(g => slotIndices[g.ValueSlot]).ToImmutableArray();
        _comparers = groups.Select(g => g.Comparer).ToImmutableArray();

        // The aggregate results land in the output buffer right after the grouping columns.
        _aggregatesFactory = EmittedAggregateCompiler.Compile(aggregates, slotIndices, groups.Length);
        _aggregateCount = aggregates.Length;
    }

    public override Iterator CreateIterator(RowBuffer? outer)
    {
        var input = _input.CreateIterator(outer);
        return new EmittedStreamAggregateIterator(input, _groupIndices, _comparers, _aggregatesFactory(), _aggregateCount, outer);
    }
}
