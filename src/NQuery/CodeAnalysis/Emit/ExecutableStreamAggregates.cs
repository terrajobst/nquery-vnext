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
//
// The output buffer lays out the grouping columns first, then the aggregate results; the
// group values are copied in from the input row, the aggregate results stored by the folds.
internal sealed class ExecutableStreamAggregates : ExecutableOperator
{
    private readonly ExecutableOperator _input;
    private readonly RowBufferLayout _outputLayout;
    private readonly ImmutableArray<RowBufferColumn> _groupSourceColumns;
    private readonly ImmutableArray<RowBufferColumn> _groupOutputColumns;
    private readonly ImmutableArray<Type> _groupTypes;
    private readonly ImmutableArray<IComparer> _comparers;
    private readonly Func<CompiledAggregates> _aggregatesFactory;

    public ExecutableStreamAggregates(ImmutableArray<ValueSlot> outputValueSlots, ExecutableOperator input, ImmutableArray<LogicalComparedValue> groups, ImmutableArray<LogicalAggregatedValue> aggregates, ImmutableArray<ValueSlot> outerSlots)
        : base(outputValueSlots)
    {
        _input = input;

        // Compile against (outer ++ input) when correlated, matching the buffer the
        // iterator feeds; otherwise just the input's own slots.
        var slots = outerSlots.IsEmpty ? input.OutputValueSlots : outerSlots.AddRange(input.OutputValueSlots);
        var slotIndices = ExpressionCompiler.CreateSlotIndices(slots);

        _outputLayout = RowBufferLayout.Create(outputValueSlots);

        // Grouping columns come first in the output; each is copied from the input row.
        _groupSourceColumns = groups.Select(g => slotIndices[g.ValueSlot]).ToImmutableArray();
        _groupOutputColumns = _outputLayout.Columns.Take(groups.Length).ToImmutableArray();
        _groupTypes = groups.Select(g => g.ValueSlot.Type).ToImmutableArray();
        _comparers = groups.Select(g => g.Comparer).ToImmutableArray();

        // The aggregate results land in the output columns after the grouping columns.
        var aggregateColumns = _outputLayout.Columns.Skip(groups.Length).ToImmutableArray();
        var aggregateTypes = aggregates.Select(a => a.Output.Type).ToImmutableArray();
        _aggregatesFactory = AggregateCompiler.Compile(aggregates, slotIndices, aggregateColumns, aggregateTypes);
    }

    public override Iterator CreateIterator(RowBuffer? outer)
    {
        var input = _input.CreateIterator(outer);
        return new StreamAggregateIterator(input, _groupSourceColumns, _groupOutputColumns, _groupTypes, _comparers, _aggregatesFactory(), _outputLayout, outer);
    }
}
