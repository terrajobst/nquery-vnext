using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NQuery.Symbols
{
    public abstract class TableSymbol : Symbol
    {
        internal TableSymbol(string name, IEnumerable<ColumnSymbol> columns)
            : base(name)
        {
            Columns = columns.ToImmutableArray();
        }

        public ImmutableArray<ColumnSymbol> Columns { get; }
    }
}