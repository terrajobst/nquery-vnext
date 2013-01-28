using System;

using NQuery.Binding;

namespace NQuery.Symbols
{
    public sealed class QueryColumnInstanceSymbol : ColumnInstanceSymbol
    {
        private readonly ValueSlot _valueSlot;

        internal QueryColumnInstanceSymbol(string name, ValueSlot valueSlot)
            : base(name)
        {
            _valueSlot = valueSlot;
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.QueryColumnInstance; }
        }

        internal override ValueSlot ValueSlot
        {
            get { return _valueSlot; }
        }
    }
}