using NQuery.Metadata.Aggregation;

namespace NQuery.Metadata;

public abstract class AggregateDefinition
{
    private protected AggregateDefinition()
    {
    }

    public abstract string Name { get; }
    internal abstract IAggregatable? CreateAggregatable(Type argumentType);
}
