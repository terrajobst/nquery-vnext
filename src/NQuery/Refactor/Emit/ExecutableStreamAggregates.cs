#nullable enable

using System.Collections;
using System.Collections.Immutable;

using NQuery.Refactor.Algebra;
using NQuery.Refactor.Binding;
using NQuery.Refactor.Iterators;
using NQuery.Symbols.Aggregation;

namespace NQuery.Refactor.Emit
{
    // Grouping + aggregation, executed as a stream aggregate over input the planner has
    // sorted on the grouping columns. The argument expressions and the grouping-column
    // positions are resolved once here; the aggregators -- which hold per-run state --
    // are created fresh in CreateIterator. Choosing a hash aggregate instead is a later
    // refinement.
    internal sealed class ExecutableStreamAggregates : ExecutableOperator
    {
        private readonly ExecutableOperator _input;
        private readonly ImmutableArray<int> _groupIndices;
        private readonly ImmutableArray<IComparer> _comparers;
        private readonly ImmutableArray<IAggregatable> _aggregatables;
        private readonly ImmutableArray<EmittedFunction> _argumentFunctions;

        public ExecutableStreamAggregates(ImmutableArray<ValueSlot> outputValueSlots, ExecutableOperator input, ImmutableArray<BoundComparedValue> groups, ImmutableArray<LogicalAggregatedValue> aggregates, ImmutableArray<ValueSlot> outerSlots)
            : base(outputValueSlots)
        {
            _input = input;

            // Compile against (outer ++ input) when correlated, matching the buffer the
            // iterator feeds; otherwise just the input's own slots.
            var slots = outerSlots.IsEmpty ? input.OutputValueSlots : outerSlots.AddRange(input.OutputValueSlots);
            var slotIndices = EmittedExpressionCompiler.CreateSlotIndices(slots);

            _groupIndices = groups.Select(g => slotIndices[g.ValueSlot]).ToImmutableArray();
            _comparers = groups.Select(g => g.Comparer).ToImmutableArray();
            _aggregatables = aggregates.Select(a => a.Aggregatable).ToImmutableArray();
            _argumentFunctions = aggregates
                                 .Select(a => EmittedExpressionCompiler.CompileFunction(a.Argument, slotIndices))
                                 .ToImmutableArray();
        }

        public override Iterator CreateIterator(RowBuffer? outer)
        {
            var input = _input.CreateIterator(outer);
            var aggregators = _aggregatables.Select(a => a.CreateAggregator()).ToImmutableArray();
            return new EmittedStreamAggregateIterator(input, _groupIndices, _comparers, aggregators, _argumentFunctions, outer);
        }
    }
}
