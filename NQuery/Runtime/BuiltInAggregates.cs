using System.Collections.Generic;
using NQuery.Language.Symbols;

namespace NQuery.Language.Runtime
{
    internal static class BuiltInAggregates
    {
        public static IEnumerable<AggregateSymbol> GetAggregates()
        {
            return new[]
            {
                new AggregateSymbol("COUNT"),
                new AggregateSymbol("AVG"),
                new AggregateSymbol("FIRST"),
                new AggregateSymbol("LAST"),
                new AggregateSymbol("MAX"),
                new AggregateSymbol("MIN"),
                new AggregateSymbol("SUM"),
                new AggregateSymbol("STDEV"),
                new AggregateSymbol("VAR"),
                new AggregateSymbol("CONCAT"),
            };
        }
    }
}