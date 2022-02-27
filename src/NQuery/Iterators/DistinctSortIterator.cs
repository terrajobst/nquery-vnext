using System.Collections;

namespace NQuery.Iterators
{
    internal sealed class DistinctSortIterator : SortIterator
    {
        private object[] _lastSpooledRow;

        public DistinctSortIterator(Iterator input, IEnumerable<RowBufferEntry> sortEntries, IEnumerable<IComparer> comparers)
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

                for (var i = 0; i < SortIndices.Length; i++)
                {
                    var entryIndex = SortIndices[i];
                    var comparer = Comparers[i];
                    var valueOfLastRow = _lastSpooledRow[entryIndex];
                    var valueOfThisRow = currentRow[entryIndex];

                    if (valueOfLastRow == valueOfThisRow)
                        continue;

                    if (comparer.Compare(valueOfLastRow, valueOfThisRow) == 0)
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
}