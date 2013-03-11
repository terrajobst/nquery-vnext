using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NQuery.Symbols
{
    public abstract class TableDefinition
    {
        private ReadOnlyCollection<ColumnDefinition> _columns;

        public ReadOnlyCollection<ColumnDefinition> Columns
        {
            get { return _columns ?? (_columns = new ReadOnlyCollection<ColumnDefinition>(GetColumns().ToArray())); }
        }

        public abstract string Name { get; }
        public abstract Type RowType { get; }

        protected abstract IEnumerable<ColumnDefinition> GetColumns();

        public abstract IEnumerable GetRows();
    }
}