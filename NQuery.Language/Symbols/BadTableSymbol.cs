using System;
using NQuery.Language.Binding;

namespace NQuery.Language.Symbols
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