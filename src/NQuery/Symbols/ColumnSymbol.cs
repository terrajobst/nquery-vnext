namespace NQuery.Symbols
{
    public abstract class ColumnSymbol : Symbol
    {
        internal ColumnSymbol(string name)
            : base(name)
        {
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.Column; }
        }
    }
}