namespace NQuery.Symbols
{
    public class ColumnSymbol : Symbol
    {
        internal ColumnSymbol(string name, Type type)
            : base(name)
        {
            ArgumentNullException.ThrowIfNull(type);

            Type = type;
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.Column; }
        }

        public override Type Type { get; }
    }
}