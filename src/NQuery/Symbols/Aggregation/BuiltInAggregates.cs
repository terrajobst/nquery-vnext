namespace NQuery.Symbols.Aggregation
{
    internal static class BuiltInAggregates
    {
        public static IEnumerable<AggregateSymbol> GetAggregates()
        {
            return new[]
            {
                Count,
                new AvgAggregateSymbol(),
                new MaxAggregateSymbol(),
                new MinAggregateSymbol(),
                new SumAggregateSymbol(),
                new StdDevAggregateSymbol(),
                new VarAggregateSymbol(),
                new ConcatAggregateSymbol()
            };
        }

        public static readonly AggregateSymbol Count = new CountAggregateSymbol();
        public static readonly AggregateSymbol Any = new AnyAggregateSymbol();
    }
}