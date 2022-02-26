using NQuery.Symbols.Aggregation;

namespace NQuery.Tests.Symbols.Aggregation
{
    public abstract class AggregateTests
    {
        internal void AssertProduces(object expected, Type argumentType, object[] values)
        {
            var aggregator = CreateAggregateDefinition().CreateAggregatable(argumentType).CreateAggregator();
            aggregator.Initialize();

            foreach (var value in values)
                aggregator.Accumulate(value);

            var actual = aggregator.GetResult();
            Assert.Equal(expected, actual);
        }

        protected abstract AggregateDefinition CreateAggregateDefinition();
    }
}
