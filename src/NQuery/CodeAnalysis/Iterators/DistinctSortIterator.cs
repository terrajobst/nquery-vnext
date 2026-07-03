namespace NQuery.CodeAnalysis.Iterators;

internal sealed class DistinctSortIterator : SortIterator
{
    private const int NoRow = -1;

    private int _lastRow = NoRow;

    public DistinctSortIterator(Iterator input, IEnumerable<RowBufferEntry> sortEntries, IEnumerable<System.Collections.IComparer> comparers)
        : base(input, sortEntries, comparers)
    {
        ThrowIfNull(input);
        ThrowIfNull(sortEntries);
        ThrowIfNull(comparers);
    }

    public override void Open()
    {
        base.Open();
        _lastRow = NoRow;
    }

    public override bool Read()
    {
        if (_lastRow == NoRow)
        {
            if (!base.Read())
                return false;

            _lastRow = CurrentRow;
            return true;
        }

        while (true)
        {
            if (!base.Read())
                return false;

            var currentRow = CurrentRow;

            // A non-zero comparison means the keys differ -- a new distinct row.
            if (CompareRows(_lastRow, currentRow) != 0)
            {
                _lastRow = currentRow;
                return true;
            }
        }
    }
}
