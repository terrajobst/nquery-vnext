using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NQuery.Symbols
{
    public abstract class TableDefinition
    {
        private ImmutableArray<ColumnDefinition> _columns;

        public ImmutableArray<ColumnDefinition> Columns
        {
            get
            {
                if (_columns.IsDefault)
                    _columns = GetColumns().ToImmutableArray();
                
                return _columns;
            }
        }

        public abstract string Name { get; }
        public abstract Type RowType { get; }

        protected abstract IEnumerable<ColumnDefinition> GetColumns();

        public abstract IEnumerable GetRows();
    }
}