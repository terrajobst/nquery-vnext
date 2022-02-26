using NQuery.Iterators;

namespace NQuery.Tests.Iterators
{
    internal sealed class MockedIterator : Iterator
    {
        private readonly IReadOnlyList<object[]> _rows;
        private readonly MockedRowBuffer _rowBuffer;

        private int _rowIndex;

        public MockedIterator(object[] rows)
        {
            if (rows.Any(v => v is object[]))
                throw new ArgumentException("Nested array detected. Use two-dimensional array for multiple columns.");

            _rows = rows.Select(v => new[] { v }).ToArray();
            _rowBuffer = new MockedRowBuffer(1);
        }

        public MockedIterator(object[,] rows)
        {
            var rowCount = rows.GetLength(0);
            var entryCount = rows.GetLength(1);

            var rowArray = new object[rowCount][];

            for (var i = 0; i < rowCount; i++)
            {
                rowArray[i] = new object[entryCount];

                for (var j = 0; j < entryCount; j++)
                {
                    rowArray[i][j] = rows[i, j];
                }
            }

            _rows = rowArray;
            _rowBuffer = new MockedRowBuffer(entryCount);
        }

        public override RowBuffer RowBuffer => _rowBuffer;

        public int DisposalCount { get; private set; }

        public int TotalOpenCount { get; private set; }

        public int TotalReadCount { get; private set; }

        public override void Open()
        {
            TotalOpenCount++;
            _rowIndex = 0;
        }

        public override void Dispose()
        {
            DisposalCount++;
        }

        public override bool Read()
        {
            if (_rowIndex == _rows.Count)
                return false;

            _rowBuffer.Value = _rows[_rowIndex];

            TotalReadCount++;
            _rowIndex++;
            return true;
        }
    }
}