namespace NQuery.Symbols.Aggregation
{
    public sealed class MinAggregateSymbol : MinMaxAggregateSymbol
    {
        public MinAggregateSymbol()
            : base(true)
        {
        }
    }
}