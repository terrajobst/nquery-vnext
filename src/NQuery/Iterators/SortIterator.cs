using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NQuery.Iterators
{
    internal sealed class SortIterator : Iterator
    {
        private readonly Iterator _input;
        private readonly ReadOnlyCollection<int> _sortEntries;
        private readonly ReadOnlyCollection<IComparer> _comparers;

        private readonly SpooledRowBuffer _spooledRowBuffer;

        public SortIterator(Iterator input, IList<int> sortEntries, IList<IComparer> comparers)
        {
            _input = input;
            _sortEntries = new ReadOnlyCollection<int>(sortEntries);
            _comparers = new ReadOnlyCollection<IComparer>(comparers);
            _spooledRowBuffer = new SpooledRowBuffer(_input.RowBuffer.Count);
        }

        public override RowBuffer RowBuffer
        {
            get { return _spooledRowBuffer; }
        }

        private IReadOnlyList<object[]> SortInput()
        {
            var result = new List<object[]>();

            while (_input.Read())
            {
                var rowValues = new object[_input.RowBuffer.Count];
                for (var i = 0; i < rowValues.Length; i++)
                    rowValues[i] = _input.RowBuffer[i];

                result.Add(rowValues);
            }

            var rowComparer = new RowComparer(_sortEntries, _comparers);
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
            private readonly int _count;

            public SpooledRowBuffer(int count)
            {
                _count = count;
            }

            public IReadOnlyList<object[]> Rows { get; set; }

            public int RowIndex { get; set; }

            public override int Count
            {
                get { return _count; }
            }

            public override object this[int index]
            {
                get { return Rows[RowIndex][index]; }
            }
        }

        private sealed class RowComparer : IComparer<object[]>
        {
            private readonly ReadOnlyCollection<int> _sortEntries;
            private readonly ReadOnlyCollection<IComparer> _comparers;

            public RowComparer(ReadOnlyCollection<int> sortEntries, ReadOnlyCollection<IComparer> comparers)
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
                while (index < _sortEntries.Count && result == 0)
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