using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

using NQuery.Symbols;

namespace NQuery.Iterators
{
    internal sealed class TableIterator : Iterator
    {
        private readonly TableDefinition _table;
        private readonly ImmutableArray<ColumnDefinition> _definedValues;
        private readonly ArrayRowBuffer _rowBuffer;

        private IEnumerator _rows;

        public TableIterator(TableDefinition table, IEnumerable<ColumnDefinition> definedValues)
        {
            _table = table;
            _definedValues = definedValues.ToImmutableArray();
            _rowBuffer = new ArrayRowBuffer(_definedValues.Length);
        }

        public override RowBuffer RowBuffer
        {
            get { return _rowBuffer; }
        }

        public override void Open()
        {
            if (_rows != null)
                Dispose();

            _rows = _table.GetRows().GetEnumerator();
        }

        public override bool Read()
        {
            if (!_rows.MoveNext())
                return false;

            for (var i = 0; i < _definedValues.Length; i++)
                _rowBuffer.Array[i] = _definedValues[i].GetValue(_rows.Current);

            return true;
        }

        public override void Dispose()
        {
            var disposable = _rows as IDisposable;
            if (disposable != null)
                disposable.Dispose();

            _rows = null;
            base.Dispose();
        }
    }
}