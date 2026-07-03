using System.Collections;
using System.Collections.Immutable;

namespace NQuery.CodeAnalysis.Iterators;

// Stream aggregate: collapses runs of consecutive equal-group rows into one output
// row. It relies on the input arriving sorted on the grouping columns (the planner
// inserts a sort to guarantee that), so it only ever holds a single group's state.
//
// Like the other emitted iterators, the argument functions take the row buffer as a
// parameter and are compiled once. When this aggregate is the correlated part of an
// Apply's right side, those functions reference the outer (left) row, so the outer
// buffer is prepended to the input buffer -- matching the (outer ++ input) slot
// layout the functions and group columns were resolved against.
//
// The output row buffer lays out the group values first, then the aggregate results.
// Group values are copied in from the input row; the aggregates store their own results.
internal sealed class StreamAggregateIterator : Iterator
{
    private readonly Iterator _input;
    private readonly ImmutableArray<RowBufferColumn> _groupSourceColumns;
    private readonly ImmutableArray<RowBufferColumn> _groupOutputColumns;
    private readonly ImmutableArray<GroupKeyComparer> _groupComparers;
    private readonly CompiledAggregates _aggregates;
    private readonly RowBuffer _readRowBuffer;
    private readonly ArrayRowBuffer _rowBuffer;

    private bool _eof;
    private bool _isFirstRecord;

    public StreamAggregateIterator(Iterator input, ImmutableArray<RowBufferColumn> groupSourceColumns, ImmutableArray<RowBufferColumn> groupOutputColumns, ImmutableArray<Type> groupTypes, ImmutableArray<IComparer> comparers, CompiledAggregates aggregates, RowBufferLayout outputLayout, RowBuffer? outer)
    {
        ThrowIfNull(input);
        ThrowIfNull(aggregates);
        ThrowIfNull(outputLayout);

        _input = input;
        _groupSourceColumns = groupSourceColumns;
        _groupOutputColumns = groupOutputColumns;

        // One typed comparer per grouping column, so the same-group test below compares the
        // group key unboxed (see GroupKeyComparer); source and output columns share a kind.
        _groupComparers = [.. groupSourceColumns.Select((c, i) => GroupKeyComparer.Create(groupTypes[i], c.Kind, comparers[i]))];
        _aggregates = aggregates;
        _readRowBuffer = outer is null ? input.RowBuffer : new CombinedRowBuffer(outer, input.RowBuffer);
        _rowBuffer = new ArrayRowBuffer(outputLayout);
    }

    public override RowBuffer RowBuffer => _rowBuffer;

    public override void Open()
    {
        _input.Open();
        _eof = !_input.Read();
        _isFirstRecord = true;
    }

    public override void Dispose() => _input.Dispose();

    public override bool Read()
    {
        if (_eof)
        {
            // With no GROUP BY, an empty input still yields a single row holding the
            // aggregates over zero rows (e.g. SELECT COUNT(*) returns 0).
            if (_groupSourceColumns.Length == 0 && _isFirstRecord)
            {
                _isFirstRecord = false;
                _aggregates.Initialize();
                _aggregates.StoreResults(_rowBuffer);
                return true;
            }

            return false;
        }

        _isFirstRecord = false;

        _aggregates.Initialize();
        StoreGroupValues();
        do
        {
            _aggregates.Accumulate(_readRowBuffer);

            if (!_input.Read())
            {
                _eof = true;
                break;
            }
        }
        while (IsCurrentRowInSameGroup());

        _aggregates.StoreResults(_rowBuffer);
        return true;
    }

    private void StoreGroupValues()
    {
        for (var i = 0; i < _groupSourceColumns.Length; i++)
            _rowBuffer.CopyFrom(_readRowBuffer, _groupSourceColumns[i], _groupOutputColumns[i]);
    }

    private bool IsCurrentRowInSameGroup()
    {
        // The previously seen group key sits in the output buffer's leading columns;
        // compare it against the current input row's grouping values.
        for (var i = 0; i < _groupSourceColumns.Length; i++)
        {
            if (!_groupComparers[i].Equal(_rowBuffer, _groupOutputColumns[i], _readRowBuffer, _groupSourceColumns[i]))
                return false;
        }

        return true;
    }
}
