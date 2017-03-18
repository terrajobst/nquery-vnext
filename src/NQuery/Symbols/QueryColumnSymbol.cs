namespace NQuery.Symbols
{
    public sealed class QueryColumnSymbol : ColumnSymbol
    {
        public QueryColumnSymbol(string name, QueryColumnInstanceSymbol queryOutput)
            : base(name, queryOutput.Type)
        {
            QueryOutput = queryOutput;
        }

        public QueryColumnInstanceSymbol QueryOutput { get; }
    }
}