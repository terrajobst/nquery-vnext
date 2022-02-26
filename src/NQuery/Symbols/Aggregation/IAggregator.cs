namespace NQuery.Symbols.Aggregation
{
    public interface IAggregator
    {
        void Initialize();
        void Accumulate(object value);
        object GetResult();
    }
}