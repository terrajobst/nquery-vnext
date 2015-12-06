using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NQuery.Iterators
{
    internal sealed class TopWithTiesIterator : TopIterator
    {
        private readonly ImmutableArray<RowBufferEntry> _tieEntries;
        private readonly ImmutableArray<IComparer> _tieComparers;

        private readonly object[] _lastTieEntryValues;
        private bool _limitReached;

        public TopWithTiesIterator(Iterator input, int limit, IEnumerable<RowBufferEntry> tieEntries, IEnumerable<IComparer> tieComparers)
            : base(input, limit)
        {
            _tieEntries = tieEntries.ToImmutableArray();
            _tieComparers = tieComparers.ToImmutableArray();
            _lastTieEntryValues = new object[_tieEntries.Length];
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
                    if (InputExhausted)
                        return false;

                    _limitReached = true;
                }
                else
                {
                    for (var i = 0; i < _tieEntries.Length; i++)
                        _lastTieEntryValues[i] = _tieEntries[i].GetValue();

                    return true;
                }
            }

            // Check if the tie values of this row are equal to the one of last row.

            var allTieValuesAreEqual = true;

            for (var i = 0; i < _tieEntries.Length; i++)
            {
                var lastTieValue = _lastTieEntryValues[i];
                var thisTieValue = _tieEntries[i].GetValue();
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