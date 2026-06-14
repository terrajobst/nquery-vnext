namespace NQuery.Metadata.Aggregation;

internal interface IAggregator
{
    void Initialize();
    void Accumulate(object? value);
    object? GetResult();
}
