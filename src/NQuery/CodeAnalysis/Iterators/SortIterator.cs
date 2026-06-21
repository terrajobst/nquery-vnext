using System.Collections;
using System.Collections.Immutable;

namespace NQuery.CodeAnalysis.Iterators;

internal class SortIterator : Iterator
{
    private readonly Iterator _input;
    private readonly RowBuffer _materializationSource;
    private readonly SpooledRowBuffer _spooledRowBuffer;
    private readonly EmittedRowComparer _rowComparer;

    // The order-array comparer for the current spool: it positions two store cursors and
    // invokes the compiled delegate. Rebuilt per Open (bound to that run's store), reused by
    // DISTINCT collapsing via CompareRows.
    private RowComparer? _comparer;

    public SortIterator(Iterator input, IEnumerable<RowBufferEntry> sortEntries, IEnumerable<IComparer> comparers)
    {
        _input = input;
        var entries = sortEntries.ToImmutableArray();
        var comparerArray = comparers.ToImmutableArray();

        // Sort keys the input doesn't produce (outer references) must be captured along
        // with each row, so they are projected and appended after the input's columns in
        // the materialized row. The exposed row stays just the input's columns.
        var outerEntries = entries.Where(e => e.RowBuffer != input.RowBuffer).Distinct().ToImmutableArray();
        _materializationSource = outerEntries.IsEmpty
            ? input.RowBuffer
            : new CombinedRowBuffer(input.RowBuffer, new ProjectedRowBuffer(outerEntries));

        var sortColumns = ResolveSortColumns(entries, outerEntries, input.RowBuffer);
        var sortTypes = entries.Select(e => e.Type).ToImmutableArray();
        _spooledRowBuffer = new SpooledRowBuffer(input.RowBuffer);

        // Compile the whole multi-key comparison once, specialized to these columns and
        // comparers; the spool rows are then sorted with one typed delegate, no boxing.
        _rowComparer = EmittedRowComparerCompiler.Compile(sortColumns, sortTypes, comparerArray);
    }

    // Each sort key's column within a materialized row: input keys keep their column (the
    // input's columns lead each container); appended outer keys sit after them per kind.
    private static ImmutableArray<RowBufferColumn> ResolveSortColumns(ImmutableArray<RowBufferEntry> entries, ImmutableArray<RowBufferEntry> outerEntries, RowBuffer input)
    {
        var outerColumns = new Dictionary<RowBufferEntry, RowBufferColumn>();
        var objectIndex = input.ObjectCount;
        var bits32Index = input.Bits32Count;
        var bits64Index = input.Bits64Count;
        var bits128Index = input.Bits128Count;

        foreach (var entry in outerEntries)
        {
            var index = entry.Column.Kind switch
            {
                RowBufferColumnKind.Object => objectIndex++,
                RowBufferColumnKind.Bits32 => bits32Index++,
                RowBufferColumnKind.Bits64 => bits64Index++,
                RowBufferColumnKind.Bits128 => bits128Index++,
                _ => throw ExceptionBuilder.UnexpectedValue(entry.Column.Kind)
            };
            outerColumns.Add(entry, new RowBufferColumn(entry.Column.Kind, index));
        }

        return entries.Select(e => e.RowBuffer == input ? e.Column : outerColumns[e]).ToImmutableArray();
    }

    public override RowBuffer RowBuffer
    {
        get { return _spooledRowBuffer; }
    }

    // The store row index the cursor currently sits on (after mapping through the sort order).
    protected int CurrentRow => _spooledRowBuffer.CurrentRow;

    // Compares two spooled rows on the sort keys via the compiled delegate. DISTINCT uses it
    // to decide whether the current row opens a new group (a non-zero result).
    protected int CompareRows(int rowX, int rowY) => _comparer!.Compare(rowX, rowY);

    private void SortInput()
    {
        var store = new SpooledRowStore(_materializationSource);
        while (_input.Read())
            store.Append(_materializationSource);

        var order = new int[store.Count];
        for (var i = 0; i < order.Length; i++)
            order[i] = i;

        // Sort the index array, not the row data: the rows stay put in the columnar store
        // and only the cheap int[] is permuted.
        _comparer = new RowComparer(store, _rowComparer);
        Array.Sort(order, _comparer);

        _spooledRowBuffer.SetRows(store, order);
    }

    public override void Open()
    {
        _input.Open();
        _spooledRowBuffer.Reset();
    }

    public override void Dispose()
    {
        _input.Dispose();
    }

    public override bool Read()
    {
        if (_spooledRowBuffer.Store is null)
            SortInput();

        return _spooledRowBuffer.MoveNext();
    }

    // A cursor over the columnar spool, exposed to the consumer as the input's layout. A
    // materialized row may carry extra trailing columns (captured outer sort keys), but the
    // exposed container counts are the input's, so the consumer sees the input's shape; the
    // raw accessors read straight from the store, mapping the cursor position through the
    // sort order.
    private sealed class SpooledRowBuffer : RowBuffer
    {
        private int _position = -1;

        public SpooledRowBuffer(RowBuffer template)
        {
            ObjectCount = template.ObjectCount;
            Bits32Count = template.Bits32Count;
            Bits64Count = template.Bits64Count;
            Bits128Count = template.Bits128Count;
        }

        public SpooledRowStore? Store { get; private set; }

        private int[]? _order;

        public int CurrentRow => _order![_position];

        public void Reset()
        {
            Store = null;
            _order = null;
            _position = -1;
        }

        public void SetRows(SpooledRowStore store, int[] order)
        {
            Store = store;
            _order = order;
            _position = -1;
        }

        public bool MoveNext()
        {
            if (_position == _order!.Length - 1)
                return false;

            _position++;
            return true;
        }

        public override int ObjectCount { get; }

        public override int Bits32Count { get; }

        public override int Bits64Count { get; }

        public override int Bits128Count { get; }

        public override object? GetObject(int index) => Store!.GetObject(CurrentRow, index);

        public override uint GetBits32(int index) => Store!.GetBits32(CurrentRow, index);

        public override ulong GetBits64(int index) => Store!.GetBits64(CurrentRow, index);

        public override Int128 GetBits128(int index) => Store!.GetBits128(CurrentRow, index);

        public override bool IsNull32(int index) => Store!.IsNull32(CurrentRow, index);

        public override bool IsNull64(int index) => Store!.IsNull64(CurrentRow, index);

        public override bool IsNull128(int index) => Store!.IsNull128(CurrentRow, index);
    }

    // Adapts the compiled row comparer to Array.Sort over the order array: it owns two
    // reusable cursors onto the spool, positions them at the two rows, and invokes the
    // delegate. Comparing indices keeps the row data put.
    private sealed class RowComparer : IComparer<int>
    {
        private readonly SpooledRowStore.Cursor _x;
        private readonly SpooledRowStore.Cursor _y;
        private readonly EmittedRowComparer _comparer;

        public RowComparer(SpooledRowStore store, EmittedRowComparer comparer)
        {
            _x = store.CreateCursor();
            _y = store.CreateCursor();
            _comparer = comparer;
        }

        public int Compare(int x, int y)
        {
            _x.Row = x;
            _y.Row = y;
            return _comparer(_x, _y);
        }
    }
}
