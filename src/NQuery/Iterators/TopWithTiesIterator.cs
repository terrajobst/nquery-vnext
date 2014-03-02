using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NQuery.Iterators
{
    internal sealed class TopWithTiesIterator : TopIterator
    {
        private readonly ReadOnlyCollection<int> _tieEntries;
        private readonly ReadOnlyCollection<IComparer> _tieComparers;

        private readonly object[] _lastTieEntryValues;
        private bool _limitReached;

        public TopWithTiesIterator(Iterator input, int limit, IList<int> tieEntries, IList<IComparer> tieComparers)
            : base(input, limit)
        {
            _tieEntries = new ReadOnlyCollection<int>(tieEntries);
            _tieComparers = new ReadOnlyCollection<IComparer>(tieComparers);
            _lastTieEntryValues = new object[_tieEntries.Count];
        }

        public override void Open()
        {
            base.Open();
        
            _limitReached = false;
        }

        public override bool Read()
        {
            if (_limitReached)
            {
                Input.Read();
            }
            else
            {
                if (!base.Read())
                {
                    _limitReached = true;
                }
                else
                {
                    for (var i = 0; i < _tieEntries.Count; i++)
                        _lastTieEntryValues[i] = Input.RowBuffer[_tieEntries[i]];

                    return true;
                }
            }

            // Check if the tie values of this row are equal to the one of last row.

            var allTieValuesAreEqual = true;

            for (var i = 0; i < _tieEntries.Count; i++)
            {
                var lastTieValue = _lastTieEntryValues[i];
                var thisTieValue = Input.RowBuffer[_tieEntries[i]];
                var comparer = _tieComparers[i];

                if (comparer.Compare(lastTieValue, thisTieValue) == 0)
                    continue;

                allTieValuesAreEqual = false;
                break;
            }

            return allTieValuesAreEqual;
        }
    }
}