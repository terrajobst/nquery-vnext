using NQuery.Binding;

namespace NQuery.Symbols
{
    public sealed class QueryColumnInstanceSymbol : ColumnInstanceSymbol
    {
        internal QueryColumnInstanceSymbol(string name, ValueSlot valueSlot)
            : base(name)
        {
            ValueSlot = valueSlot;
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.QueryColumnInstance; }
        }

        internal override ValueSlot ValueSlot { get; }
    }
}