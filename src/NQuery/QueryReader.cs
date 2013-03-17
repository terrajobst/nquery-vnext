using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using NQuery.Plan;

namespace NQuery
{
    public sealed class QueryReader : IDisposable
    {
        private readonly ReadOnlyCollection<string> _columnNames;
        private readonly ReadOnlyCollection<Type> _columnTypes;
        private readonly bool _schemaOnly;

        private Iterator _iterator;
        private bool _isBof;

        internal QueryReader(Iterator iterator, IReadOnlyCollection<Tuple<string, Type>> columnNamesAndTypes, bool schemaOnly)
        {
            _iterator = iterator;
            _schemaOnly = schemaOnly;
            _columnNames = new ReadOnlyCollection<string>(columnNamesAndTypes.Select(t => t.Item1).ToArray());
            _columnTypes = new ReadOnlyCollection<Type>(columnNamesAndTypes.Select(t => t.Item2).ToArray());

            if (!_schemaOnly)
            {
                _iterator.Initialize();
                _iterator.Open();
            }

            _isBof = true;
        }

        public void Dispose()
        {
            if (_iterator == null)
                return;

            _iterator.Dispose();
            _iterator = null;
        }

        public bool Read()
        {
            if (_schemaOnly)
                return false;

            if (_iterator.Read())
            {
                _isBof = false;
                return true;
            }

            return false;
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
            get
            {
                if (_isBof || _iterator == null)
                    throw new InvalidOperationException(Resources.InvalidAttemptToRead);

                return _iterator.RowBuffer[columnIndex];
            }
        }

        public int ColumnCount
        {
            get { return _iterator.RowBuffer.Count; }
        }
    }
}