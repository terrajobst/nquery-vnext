namespace NQuery.Symbols
{
    public class ColumnSymbol : Symbol
    {
        internal ColumnSymbol(string name, Type type)
            : base(name)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            Type = type;
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.Column; }
        }

        public override Type Type { get; }
    }
}