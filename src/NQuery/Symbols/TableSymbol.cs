#nullable enable

using System;
using System.Collections.Immutable;

namespace NQuery.Symbols
{
    public abstract class TableSymbol : Symbol
    {
        protected TableSymbol(string name)
            : base(name)
        {
        }

        public abstract ImmutableArray<ColumnSymbol> Columns { get; }
    }
}