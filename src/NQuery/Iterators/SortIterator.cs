using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace NQuery.Iterators
{
    internal class SortIterator : Iterator
    {
        private readonly Iterator _input;
        private readonly SpooledRowBuffer _spooledRowBuffer;
        private readonly ImmutableArray<RowBufferEntry> _outerSortEntries;

        public SortIterator(Iterator input, IEnumerable<RowBufferEntry> sortEntries, IEnumerable<IComparer> comparers)
        {
            _input = input;
            var sortEntryArray = sortEntries.ToImmutableArray();
            Comparers = comparers.ToImmutableArray();

            // We need to have enough room to store the entire input
            // as well as all sort entries that aren't already part
            // of the input (for instance, if they come for an outer
            // reference).

            _outerSortEntries = sortEntryArray.Where(e => e.RowBuffer != _input.RowBuffer)
                                              .ToImmutableArray();

            var exposedCount = input.RowBuffer.Count;
            var spooledCount = input.RowBuffer.Count + _outerSortEntries.Length;
            _spooledRowBuffer = new SpooledRowBuffer(exposedCount, spooledCount);
            SortIndices = sortEntryArray.Select(e => e.RowBuffer == _input.RowBuffer
                                                    ? e.Index
                                                    : input.RowBuffer.Count + _outerSortEntries.IndexOf(e))
                                        .ToImmutableArray();
        }

        public override RowBuffer RowBuffer
        {
            get { return _spooledRowBuffer; }
        }

        public ImmutableArray<int> SortIndices { get; }

        public ImmutableArray<IComparer> Comparers { get; }

        protected object[] GetCurrentRow()
        {
            return _spooledRowBuffer.Rows[_spooledRowBuffer.RowIndex];
        }

        private IReadOnlyList<object[]> SortInput()
        {
            var result = new List<object[]>();

            while (_input.Read())
            {
                var rowValues = new object[_spooledRowBuffer.SpooledCount];

                // First, we copy the input

                _input.RowBuffer.CopyTo(rowValues, 0);

                // Now we copy the remainder

                for (var i = 0; i < _outerSortEntries.Length; i++)
                {
                    var targetIndex = _input.RowBuffer.Count + i;
                    rowValues[targetIndex] = _outerSortEntries[i].GetValue();
                }

                result.Add(rowValues);
            }

            var rowComparer = new RowComparer(SortIndices, Comparers);
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
            if (_spooledRowBuffer.Rows == null)
            {
                _spooledRowBuffer.Rows = SortInput();
                _spooledRowBuffer.RowIndex = -1;
            }

            if (_spooledRowBuffer.RowIndex == _spooledRowBuffer.Rows.Count - 1)
                return false;

            _spooledRowBuffer.RowIndex++;
            return true;
        }

        private sealed class SpooledRowBuffer : RowBuffer
        {
            public SpooledRowBuffer(int exposedCount, int spooledCount)
            {
                Count = exposedCount;
                SpooledCount = spooledCount;
            }

            public IReadOnlyList<object[]> Rows { get; set; }

            public int RowIndex { get; set; }

            public override int Count { get; }

            public int SpooledCount { get; }

            public override object this[int index]
            {
                get { return Rows[RowIndex][index]; }
            }

            public override void CopyTo(object[] array, int destinationIndex)
            {
                var source = Rows[RowIndex];
                Array.Copy(source, 0, array, destinationIndex, Count);
            }
        }

        private sealed class RowComparer : IComparer<object[]>
        {
            private readonly ImmutableArray<int> _sortEntries;
            private readonly ImmutableArray<IComparer> _comparers;

            public RowComparer(ImmutableArray<int> sortEntries, ImmutableArray<IComparer> comparers)
            {
                _sortEntries = sortEntries;
                _comparers = comparers;
            }

            public int Compare(object[] x, object[] y)
            {
                // Compare all columns

                var result = 0;
                var index = 0;
                while (index < _sortEntries.Length && result == 0)
                {
                    var valueIndex = _sortEntries[index];

                    var value1 = x[valueIndex];
                    var value2 = y[valueIndex];

                    if (value1 == null && value2 != null)
                        return -1;

                    if (value1 != null && value2 == null)
                        return +1;

                    if (value1 != null && value2 != null)
                    {
                        result = _comparers[index].Compare(value1, value2);

                        if (result != 0)
                            return result;
                    }

                    index++;
                }

                return 0;
            }
        }
    }
}