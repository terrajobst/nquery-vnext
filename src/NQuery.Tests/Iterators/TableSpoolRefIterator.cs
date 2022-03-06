using NQuery.Iterators;

namespace NQuery.Tests.Iterators
{
    public class TableSpoolRefIteratorTests : IteratorTests
    {
        [Fact]
        public void Iterators_TableSpoolRef_ReturnsEmpty_IfStackIsEmpty()
        {
            var stack = new TableSpoolStack(1);

            using var iterator = new TableSpoolRefIterator(stack);
            AssertEmpty(iterator);
        }

        [Fact]
        public void Iterators_TableSpoolRef_ReturnsStack_AndPops()
        {
            var stack = new TableSpoolStack(2);
            stack.Push(new MockedRowBuffer(new object[] { 1, "One" }));
            stack.Push(new MockedRowBuffer(new object[] { 2, "Two" }));

            var expected = new object[,]
            {
                {2, "Two"},
                {1, "One"}
            };

            using var iterator = new TableSpoolRefIterator(stack);
            AssertProduces(iterator, expected);

            Assert.True(stack.IsEmpty);
        }
    }
}