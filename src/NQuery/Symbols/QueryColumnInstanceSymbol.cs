using System;

using NQuery.Binding;

namespace NQuery.Symbols
{
    public sealed class QueryColumnInstanceSymbol : ColumnInstanceSymbol
    {
        internal QueryColumnInstanceSymbol(ColumnSymbol column, string name, ValueSlot valueSlot)
            : base(name)
        {
            Column = column;
            ValueSlot = valueSlot;
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.QueryColumnInstance; }
        }

        public ColumnSymbol Column { get; }

        internal override ValueSlot ValueSlot { get; }
    }
}