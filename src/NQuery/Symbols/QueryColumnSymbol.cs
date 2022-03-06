namespace NQuery.Symbols
{
    public sealed class QueryColumnSymbol : ColumnSymbol
    {
        internal QueryColumnSymbol(string name, QueryColumnInstanceSymbol queryColumnInstance)
            : base(name)
        {
            ArgumentNullException.ThrowIfNull(queryColumnInstance);

            QueryColumnInstance = queryColumnInstance;
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.Column; }
        }

        public override Type Type => QueryColumnInstance.Type;

        public QueryColumnInstanceSymbol QueryColumnInstance { get; }
    }
}