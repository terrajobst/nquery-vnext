using NQuery.Metadata;

namespace NQuery.Tests.CodeAnalysis.Symbols.Aggregation;

public abstract class AggregateTests
{
    internal void AssertProduces(object? expected, Type argumentType, object?[] values)
    {
        var fold = CreateAggregateDefinition().CreateFold(argumentType)!;
        var seed = fold.Seed.Compile();
        var accumulate = fold.Accumulate.Compile();
        var result = fold.Result.Compile();

        var state = seed.DynamicInvoke();
        foreach (var value in values)
        {
            if (value is not null)
                state = accumulate.DynamicInvoke(state, value);
        }

        var actual = result.DynamicInvoke(state);
        Assert.Equal(expected, actual);
    }

    protected abstract AggregateDefinition CreateAggregateDefinition();
}
