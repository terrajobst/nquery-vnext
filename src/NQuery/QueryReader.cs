using System.Collections.Immutable;

using NQuery.Iterators;

namespace NQuery
{
    public sealed class QueryReader : IDisposable
    {
        private readonly ImmutableArray<string> _columnNames;
        private readonly ImmutableArray<Type> _columnTypes;
        private readonly bool _schemaOnly;

        private Iterator _iterator;
        private bool _isBof;

        internal QueryReader(Iterator iterator, ImmutableArray<Tuple<string, Type>> columnNamesAndTypes, bool schemaOnly)
        {
            _iterator = iterator;
            _schemaOnly = schemaOnly;
            _columnNames = columnNamesAndTypes.Select(t => t.Item1).ToImmutableArray();
            _columnTypes = columnNamesAndTypes.Select(t => t.Item2).ToImmutableArray();

            if (!_schemaOnly)
                _iterator.Open();

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