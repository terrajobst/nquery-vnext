using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NQuery.Symbols
{
    public sealed class DerivedTableSymbol : TableSymbol
    {
        internal DerivedTableSymbol(IEnumerable<ColumnSymbol> columns)
            : base(string.Empty)
        {
            Columns = columns.ToImmutableArray();
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.DerivedTable; }
        }

        public override Type Type
        {
            get { return TypeFacts.Missing; }
        }

        public override ImmutableArray<ColumnSymbol> Columns { get; }
    }
}