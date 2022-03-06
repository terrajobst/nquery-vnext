using NQuery.Iterators;

namespace NQuery.Tests.Iterators
{
    public class ProjectionIteratorTests : IteratorTests
    {
        [Fact]
        public void Iterators_Projection_ForwardsProperly()
        {
            var rows = new object[] { 1, 2 };
            var expected = rows;

            using var input = new MockedIterator(rows);
            var entries = new[]
            {
                new RowBufferEntry(input.RowBuffer, 0)
            };

            using (var iterator = new ProjectionIterator(input, entries))
            {
                for (var i = 0; i < 2; i++)
                {
                    AssertProduces(iterator, expected);
                }
            }

            Assert.Equal(2, input.TotalOpenCount);
            Assert.Equal(4, input.TotalReadCount);
            Assert.Equal(1, input.DisposalCount);
        }

        [Fact]
        public void Iterators_Projection_ReturnsEmpty_IfInputEmpty()
        {
            var rows = Array.Empty<object>();

            using var input = new MockedIterator(rows);
            using var iterator = new ProjectionIterator(input, Enumerable.Empty<RowBufferEntry>());
            AssertEmpty(iterator);
        }

        [Fact]
        public void Iterators_Projection_SwapsEntries()
        {
            var rows = new object[,]
            {
                {1, "One"},
                {2, "Two"}
            };

            var expected = new object[,]
            {
                {"One", 1},
                {"Two", 2}
            };

            using var input = new MockedIterator(rows);
            var entries = new[]
            {
                new RowBufferEntry(input.RowBuffer, 1),
                new RowBufferEntry(input.RowBuffer, 0)
            };

            using var iterator = new ProjectionIterator(input, entries);
            AssertProduces(iterator, expected);
        }

        [Fact]
        public void Iterators_Projection_RemovesEntries()
        {
            var rows = new object[,]
            {
                {1, "One"},
                {2, "Two"}
            };

            var expected = new object[] { "One", "Two" };

            using var input = new MockedIterator(rows);
            var entries = new[]
            {
                new RowBufferEntry(input.RowBuffer, 1)
            };

            using var iterator = new ProjectionIterator(input, entries);
            AssertProduces(iterator, expected);
        }
    }
}