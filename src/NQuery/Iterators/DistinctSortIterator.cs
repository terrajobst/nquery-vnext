using System;
using System.Collections;
using System.Collections.Generic;

namespace NQuery.Iterators
{
    internal sealed class DistinctSortIterator : SortIterator
    {
        private object[] _lastSpooledRow;

        public DistinctSortIterator(Iterator input, IEnumerable<int> sortEntries, IEnumerable<IComparer> comparers)
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
            if (_lastSpooledRow == null)
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

                for (var i = 0; i < SortEntries.Length; i++)
                {
                    var entryIndex = SortEntries[i];
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