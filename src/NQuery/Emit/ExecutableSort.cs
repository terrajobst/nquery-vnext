using System.Collections.Immutable;

using NQuery.Algebra;
using NQuery.Iterators;

namespace NQuery.Emit;

internal sealed class ExecutableSort : ExecutableOperator
{
    private readonly ExecutableOperator _input;
    private readonly bool _isDistinct;
    private readonly ImmutableArray<LogicalComparedValue> _sortedValues;

    public ExecutableSort(ImmutableArray<ValueSlot> outputValueSlots, ExecutableOperator input, bool isDistinct, ImmutableArray<LogicalComparedValue> sortedValues)
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
