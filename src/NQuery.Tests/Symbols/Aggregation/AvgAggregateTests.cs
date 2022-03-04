using NQuery.Symbols.Aggregation;

namespace NQuery.Tests.Symbols.Aggregation
{
    public sealed class AvgAggregateTests : AggregateTests
    {
        [Fact]
        public void Aggregates_Avg_ReturnsNull_IfInputIsEmpty()
        {
            var values = new object[0];
            AssertProduces(null, typeof(int), values);
        }

        protected override AggregateDefinition CreateAggregateDefinition()
        {
            return new AvgAggregateDefinition();
        }
    }
}