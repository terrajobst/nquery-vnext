namespace NQuery.CodeAnalysis.Iterators;

internal sealed class DistinctSortIterator : SortIterator
{
    private ArrayRowBuffer? _lastSpooledRow;

    public DistinctSortIterator(Iterator input, IEnumerable<RowBufferEntry> sortEntries, IEnumerable<System.Collections.IComparer> comparers)
        : base(input, sortEntries, comparers)
    {
    }

    public override void Open()
    {
        base.Open();
        _lastSpooledRow = null;
    }

    public override bool Read()
    {
        if (_lastSpooledRow is null)
        {
            if (!base.Read())
                return false;

            _lastSpooledRow = GetCurrentRow();
            return true;
        }

        var atLeastOneRecordFound = false;

        while (true)
        {
            if (!base.Read())
                break;

            var currentRow = GetCurrentRow();

            for (var i = 0; i < SortColumns.Length; i++)
            {
                var valueOfLastRow = _lastSpooledRow.GetBoxedValue(SortColumns[i], SortTypes[i]);
                var valueOfThisRow = currentRow.GetBoxedValue(SortColumns[i], SortTypes[i]);

                if (Equals(valueOfLastRow, valueOfThisRow))
                    continue;

                if (Comparers[i].Compare(valueOfLastRow, valueOfThisRow) == 0)
                    continue;

                atLeastOneRecordFound = true;
                break;
            }

            if (atLeastOneRecordFound)
            {
                _lastSpooledRow = currentRow;
                break;
            }
        }

        return atLeastOneRecordFound;
    }
}
