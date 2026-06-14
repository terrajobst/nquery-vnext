namespace NQuery.Metadata.Aggregation;

internal sealed class MinAggregateDefinition : MinMaxAggregateDefinition
{
    public MinAggregateDefinition()
        : base(true)
    {
    }
}
