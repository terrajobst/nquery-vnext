using System;

using NQuery.Plan;

namespace NQuery
{
    public sealed class QueryReader : IDisposable
    {
        private readonly Iterator _iterator;

        internal QueryReader(Iterator iterator)
        {
            _iterator = iterator;
            _iterator.Initialize();
            _iterator.Open();
        }

        public void Dispose()
        {
            _iterator.Dispose();
        }

        public bool Read()
        {
            return _iterator.Read();
        }

        public string GetColumnName(int columnIndex)
        {
            // TODO: Implement this
            return string.Empty;
        }

        public Type GetColumnType(int columnIndex)
        {
            // TODO: Implement this
            return typeof(string);
        }

        public object this[int columnIndex]
        {
            get { return _iterator.RowBuffer[columnIndex]; }
        }

        public int ColumnCount
        {
            get { return _iterator.RowBuffer.Count; }
        }
    }
}