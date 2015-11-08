using System;

namespace NQuery.Symbols
{
    public sealed class ErrorTableSymbol : TableSymbol
    {
        internal ErrorTableSymbol(string name)
            : base(name, new ColumnSymbol[0])
        {
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.ErrorTable; }
        }

        public override Type Type
        {
            get { return TypeFacts.Unknown; }
        }
    }
}