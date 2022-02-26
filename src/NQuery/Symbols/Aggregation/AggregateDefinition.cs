namespace NQuery.Symbols.Aggregation
{
    public abstract class AggregateDefinition
    {
        public abstract string Name { get; }
        public abstract IAggregatable CreateAggregatable(Type argumentType);
    }
}