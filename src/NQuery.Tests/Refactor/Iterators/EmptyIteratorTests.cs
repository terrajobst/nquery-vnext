using NQuery.Refactor.Iterators;

namespace NQuery.Tests.Refactor.Iterators
{
    public class EmptyIteratorTests : IteratorTests
    {
        [Fact]
        public void Iterators_Empty_ReturnsNoRows()
        {
            using var iterator = new EmptyIterator();
            AssertEmpty(iterator);
        }
    }
}
