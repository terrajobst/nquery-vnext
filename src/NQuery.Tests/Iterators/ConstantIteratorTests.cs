using NQuery.Iterators;

namespace NQuery.Tests.Iterators
{
    public class ConstantIteratorTests : IteratorTests
    {
        [Fact]
        public void Iterators_Constant_ReturnsSingleRow()
        {
            var expected = new object[1, 0];

            using var iterator = new ConstantIterator();
            AssertProduces(iterator, expected);
        }
    }
}