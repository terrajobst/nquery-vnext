#nullable enable

using System.Collections.Immutable;

using NQuery.Binding;
using NQuery.EmittedIterators;

namespace NQuery.Emit
{
    internal sealed class ExecutableSort : ExecutableOperator
    {
        private readonly ExecutableOperator _input;
        private readonly bool _isDistinct;
        private readonly ImmutableArray<BoundComparedValue> _sortedValues;

        public ExecutableSort(ImmutableArray<ValueSlot> outputValueSlots, ExecutableOperator input, bool isDistinct, ImmutableArray<BoundComparedValue> sortedValues)
            : base(outputValueSlots)
        {
            _input = input;
            _isDistinct = isDistinct;
            _sortedValues = sortedValues;
        }

        public override Iterator CreateIterator(RowBuffer? outer)
        {
            var input = _input.CreateIterator(outer);
            var allocation = Allocate(_input, input);
            var entries = _sortedValues.Select(v => allocation[v.ValueSlot]).ToImmutableArray();
            var comparers = _sortedValues.Select(v => v.Comparer).ToImmutableArray();
            return _isDistinct
                ? new DistinctSortIterator(input, entries, comparers)
                : new SortIterator(input, entries, comparers);
        }
    }
}
