using NQuery.Symbols.Aggregation;

namespace NQuery.Tests.Symbols.Aggregation
{
    public sealed class AvgAggregateTests : AggregateTests
    {
        [Fact]
        public void Aggregates_Avg_ReturnsNull_IfInputIsEmpty()
        {
            var values = Array.Empty<object>();
            AssertProduces(null, typeof(int), values);
        }

        protected override AggregateSymbol CreateAggregateSymbol()
        {
            return new AvgAggregateSymbol();
        }
    }
}