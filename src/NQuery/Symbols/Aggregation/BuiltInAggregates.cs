#nullable enable

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
                Count,
                new AggregateSymbol(new AvgAggregateDefinition()),
                new AggregateSymbol(new MaxAggregateDefinition()),
                new AggregateSymbol(new MinAggregateDefinition()),
                new AggregateSymbol(new SumAggregateDefinition()),
                new AggregateSymbol(new StdDevAggregateDefinition()),
                new AggregateSymbol(new VarAggregateDefinition()),
                new AggregateSymbol(new ConcatAggregateDefinition())
            };
        }

        public static AggregateSymbol Count = new AggregateSymbol(new CountAggregateDefinition());
        public static AggregateSymbol Any = new AggregateSymbol(new AnyAggregateDefinition());
    }
}