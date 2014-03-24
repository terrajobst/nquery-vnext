using System;
using System.Collections.Generic;

namespace NQuery.Symbols.Aggregation
{
    internal static class BuiltInAggregates
    {
        public static IEnumerable<AggregateSymbol> GetAggregates()
        {
            return new[]
            {
                new AggregateSymbol(new CountAggregateDefinition()),
                new AggregateSymbol(new AvgAggregateDefinition()),
                new AggregateSymbol(new MaxAggregateDefinition()),
                new AggregateSymbol(new MinAggregateDefinition()),
                new AggregateSymbol(new SumAggregateDefinition()),
                new AggregateSymbol(new StdDevAggregateDefinition()),
                new AggregateSymbol(new VarAggregateDefinition()),
                new AggregateSymbol(new ConcatAggregateDefinition())
            };
        }
    }
}