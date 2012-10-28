using System;

using NQuery.Binding;

namespace NQuery.Symbols
{
    public sealed class BadTableSymbol : TableSymbol
    {
        public BadTableSymbol(string name)
            : base(name, new ColumnSymbol[0])
        {
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.BadTable; }
        }

        public override Type Type
        {
            get { return KnownTypes.Unknown; }
        }
    }
}