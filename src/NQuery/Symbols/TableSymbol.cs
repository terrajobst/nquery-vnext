using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NQuery.Symbols
{
    public abstract class TableSymbol : Symbol
    {
        private readonly ImmutableArray<ColumnSymbol> _columns;

        protected TableSymbol(string name, IEnumerable<ColumnSymbol> columns)
            : base(name)
        {
            _columns = columns.ToImmutableArray();
        }

        public ImmutableArray<ColumnSymbol> Columns
        {
            get { return _columns; }
        }
    }
}