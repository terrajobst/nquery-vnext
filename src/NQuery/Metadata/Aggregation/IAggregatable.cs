namespace NQuery.Metadata.Aggregation;

internal interface IAggregatable
{
    IAggregator CreateAggregator();
    Type ReturnType { get; }
}
