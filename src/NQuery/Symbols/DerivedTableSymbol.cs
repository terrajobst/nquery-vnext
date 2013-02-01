using System;
using System.Collections.Generic;

namespace NQuery.Symbols
{
    public sealed class DerivedTableSymbol : TableSymbol
    {
        public DerivedTableSymbol(IList<ColumnSymbol> columns)
            : base(string.Empty, columns)
        {
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.DerivedTable; }
        }

        public override Type Type
        {
            get { return TypeFacts.Missing; }
        }
    }
}