using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NQuery.Iterators
{
    internal class SortIterator : Iterator
    {
        private readonly Iterator _input;

        private readonly SpooledRowBuffer _spooledRowBuffer;

        public SortIterator(Iterator input, IEnumerable<int> sortEntries, IEnumerable<IComparer> comparers)
        {
            _input = input;
            SortEntries = sortEntries.ToImmutableArray();
            Comparers = comparers.ToImmutableArray();
            _spooledRowBuffer = new SpooledRowBuffer(_input.RowBuffer.Count);
        }

        public override RowBuffer RowBuffer
        {
            get { return _spooledRowBuffer; }
        }

        public ImmutableArray<int> SortEntries { get; }

        public ImmutableArray<IComparer> Comparers { get; }

        protected object[] GetCurrentRow()
        {
            if (_spooledRowBuffer.Rows == null)
                return null;

            return _spooledRowBuffer.Rows[_spooledRowBuffer.RowIndex];
        }

        private IReadOnlyList<object[]> SortInput()
        {
            var result = new List<object[]>();

            while (_input.Read())
            {
                var rowValues = new object[_input.RowBuffer.Count];
                _input.RowBuffer.CopyTo(rowValues, 0);
                result.Add(rowValues);
            }

            var rowComparer = new RowComparer(SortEntries, Comparers);
            result.Sort(rowComparer);
            return result;
        }

        public override void Open()
        {
            _input.Open();
            _spooledRowBuffer.Rows = null;
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
            public SpooledRowBuffer(int count)
            {
                Count = count;
            }

            public IReadOnlyList<object[]> Rows { get; set; }

            public int RowIndex { get; set; }

            public override int Count { get; }

            public override object this[int index]
            {
                get { return Rows[RowIndex][index]; }
            }

            public override void CopyTo(object[] array, int destinationIndex)
            {
                var source = Rows[RowIndex];
                Array.Copy(source, 0, array, destinationIndex, source.Length);
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
                if (x == null && y == null)
                    return 0;

                if (x == null)
                    return -1;

                if (y == null)
                    return +1;

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