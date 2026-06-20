namespace NQuery.CodeAnalysis.Iterators;

internal sealed class DistinctSortIterator : SortIterator
{
    private const int NoRow = -1;

    private int _lastRow = NoRow;

    public DistinctSortIterator(Iterator input, IEnumerable<RowBufferEntry> sortEntries, IEnumerable<System.Collections.IComparer> comparers)
        : base(input, sortEntries, comparers)
    {
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

        var atLeastOneRecordFound = false;

        while (true)
        {
            if (!base.Read())
                break;

            var currentRow = CurrentRow;

            for (var i = 0; i < SortColumns.Length; i++)
            {
                var valueOfLastRow = GetSortKey(_lastRow, i);
                var valueOfThisRow = GetSortKey(currentRow, i);

                if (Equals(valueOfLastRow, valueOfThisRow))
                    continue;

                if (Comparers[i].Compare(valueOfLastRow, valueOfThisRow) == 0)
                    continue;

                atLeastOneRecordFound = true;
                break;
            }

            if (atLeastOneRecordFound)
            {
                _lastRow = currentRow;
                break;
            }
        }

        return atLeastOneRecordFound;
    }
}
