namespace NQuery.Symbols
{
    public class ParameterSymbol : Symbol
    {
        public ParameterSymbol(string name, Type type)
            : base(name)
        {
            Type = type;
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.Parameter; }
        }

        public override Type Type { get; }
    }
}