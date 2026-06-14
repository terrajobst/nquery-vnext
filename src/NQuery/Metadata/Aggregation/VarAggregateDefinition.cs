namespace NQuery.Metadata.Aggregation;

internal sealed class VarAggregateDefinition : VarAndStdDevAggregateDefinition
{
    public VarAggregateDefinition()
        : base(true)
    {
    }
}
