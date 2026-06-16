using NQuery.CodeAnalysis.Symbols;

namespace NQuery.Metadata.Aggregation;

internal static class BuiltInAggregates
{
    public static IEnumerable<AggregateDefinition> GetAggregates()
    {
        return new AggregateDefinition[]
        {
            new CountAggregateDefinition(),
            new AvgAggregateDefinition(),
            new MaxAggregateDefinition(),
            new MinAggregateDefinition(),
            new SumAggregateDefinition(),
            new StdDevAggregateDefinition(),
            new VarAggregateDefinition(),
            new ConcatAggregateDefinition()
        };
    }

    public static readonly AggregateSymbol Count = new(new CountAggregateDefinition());
    public static readonly AggregateSymbol Any = new(new AnyAggregateDefinition());
}
