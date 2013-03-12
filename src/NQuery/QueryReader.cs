using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using NQuery.Plan;

namespace NQuery
{
    public sealed class QueryReader : IDisposable
    {
        private readonly Iterator _iterator;
        private readonly ReadOnlyCollection<string> _columnNames;
        private readonly ReadOnlyCollection<Type> _columnTypes;

        internal QueryReader(Iterator iterator, IReadOnlyCollection<Tuple<string, Type>> columnNamesAndTypes)
        {
            _iterator = iterator;
            _columnNames = new ReadOnlyCollection<string>(columnNamesAndTypes.Select(t => t.Item1).ToArray());
            _columnTypes = new ReadOnlyCollection<Type>(columnNamesAndTypes.Select(t => t.Item2).ToArray());

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
            return _columnNames[columnIndex];
        }

        public Type GetColumnType(int columnIndex)
        {
            return _columnTypes[columnIndex];
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