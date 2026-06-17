using System.Collections;
using System.Collections.Immutable;

namespace NQuery.CodeAnalysis.Iterators;

internal class SortIterator : Iterator
{
    private readonly Iterator _input;
    private readonly RowBuffer _materializationSource;
    private readonly SpooledRowBuffer _spooledRowBuffer;

    public SortIterator(Iterator input, IEnumerable<RowBufferEntry> sortEntries, IEnumerable<IComparer> comparers)
    {
        _input = input;
        var entries = sortEntries.ToImmutableArray();
        Comparers = comparers.ToImmutableArray();

        // Sort keys the input doesn't produce (outer references) must be captured along
        // with each row, so they are projected and appended after the input's columns in
        // the materialized row. The exposed row stays just the input's columns.
        var outerEntries = entries.Where(e => e.RowBuffer != input.RowBuffer).Distinct().ToImmutableArray();
        _materializationSource = outerEntries.IsEmpty
            ? input.RowBuffer
            : new CombinedRowBuffer(input.RowBuffer, new ProjectedRowBuffer(outerEntries));

        SortColumns = ResolveSortColumns(entries, outerEntries, input.RowBuffer);
        SortTypes = entries.Select(e => e.Type).ToImmutableArray();
        _spooledRowBuffer = new SpooledRowBuffer(input.RowBuffer);
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

    protected ImmutableArray<RowBufferColumn> SortColumns { get; }

    protected ImmutableArray<Type> SortTypes { get; }

    protected ImmutableArray<IComparer> Comparers { get; }

    protected ArrayRowBuffer GetCurrentRow()
    {
        return _spooledRowBuffer.Rows![_spooledRowBuffer.RowIndex];
    }

    private IReadOnlyList<ArrayRowBuffer> SortInput()
    {
        var result = new List<ArrayRowBuffer>();

        while (_input.Read())
            result.Add(_materializationSource.Clone());

        var rowComparer = new RowComparer(SortColumns, SortTypes, Comparers);
        result.Sort(rowComparer);
        return result;
    }

    public override void Open()
    {
        _input.Open();
        _spooledRowBuffer.Rows = null;
    }

    public override void Dispose()
    {
        _input.Dispose();
    }

    public override bool Read()
    {
        var rows = _spooledRowBuffer.Rows;
        if (rows is null)
        {
            rows = SortInput();
            _spooledRowBuffer.Rows = rows;
            _spooledRowBuffer.RowIndex = -1;
        }

        if (_spooledRowBuffer.RowIndex == rows.Count - 1)
            return false;

        _spooledRowBuffer.RowIndex++;
        return true;
    }

    // Exposes the materialized rows as the input's layout. A materialized row may carry
    // extra trailing columns (captured outer sort keys); only the leading input columns
    // are exposed, so the consumer sees the input's shape.
    private sealed class SpooledRowBuffer : RowBuffer
    {
        public SpooledRowBuffer(RowBuffer template)
        {
            ObjectCount = template.ObjectCount;
            Bits32Count = template.Bits32Count;
            Bits64Count = template.Bits64Count;
            Bits128Count = template.Bits128Count;
        }

        public IReadOnlyList<ArrayRowBuffer>? Rows { get; set; }

        public int RowIndex { get; set; }

        public override int ObjectCount { get; }

        public override int Bits32Count { get; }

        public override int Bits64Count { get; }

        public override int Bits128Count { get; }

        private ArrayRowBuffer Current => Rows![RowIndex];

        public override object? GetObject(int index) => Current.GetObject(index);

        public override uint GetBits32(int index) => Current.GetBits32(index);

        public override ulong GetBits64(int index) => Current.GetBits64(index);

        public override Bits128 GetBits128(int index) => Current.GetBits128(index);

        public override bool IsNull32(int index) => Current.IsNull32(index);

        public override bool IsNull64(int index) => Current.IsNull64(index);

        public override bool IsNull128(int index) => Current.IsNull128(index);
    }

    private sealed class RowComparer : IComparer<ArrayRowBuffer>
    {
        private readonly ImmutableArray<RowBufferColumn> _sortColumns;
        private readonly ImmutableArray<Type> _sortTypes;
        private readonly ImmutableArray<IComparer> _comparers;

        public RowComparer(ImmutableArray<RowBufferColumn> sortColumns, ImmutableArray<Type> sortTypes, ImmutableArray<IComparer> comparers)
        {
            _sortColumns = sortColumns;
            _sortTypes = sortTypes;
            _comparers = comparers;
        }

        public int Compare(ArrayRowBuffer? x, ArrayRowBuffer? y)
        {
            var index = 0;
            while (index < _sortColumns.Length)
            {
                var value1 = x!.GetBoxedValue(_sortColumns[index], _sortTypes[index]);
                var value2 = y!.GetBoxedValue(_sortColumns[index], _sortTypes[index]);

                if (value1 is null && value2 is not null)
                    return -1;

                if (value1 is not null && value2 is null)
                    return +1;

                if (value1 is not null && value2 is not null)
                {
                    var result = _comparers[index].Compare(value1, value2);

                    if (result != 0)
                        return result;
                }

                index++;
            }

            return 0;
        }
    }
}
