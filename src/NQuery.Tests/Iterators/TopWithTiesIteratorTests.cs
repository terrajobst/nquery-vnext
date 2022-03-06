using System.Collections;

using NQuery.Iterators;

namespace NQuery.Tests.Iterators
{
    public class TopWithTiesIteratorTests : IteratorTests
    {
        private static Iterator CreateIterator(Iterator input, int maxValue)
        {
            var entries = new[] { new RowBufferEntry(input.RowBuffer, 0) };
            var comparers = new[] { Comparer.Default };
            return new TopWithTiesIterator(input, maxValue, entries, comparers);
        }

        [Fact]
        public void Iterators_TopWithTies_ForwardsProperly()
        {
            var rows = new object[] { 1, 2 };
            var expected = rows;

            using var input = new MockedIterator(rows);
            using (var iterator = CreateIterator(input, int.MaxValue))
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
        public void Iterators_TopWithTies_ReturnsEmpty_IfInputIsEmpty()
        {
            var rows = Array.Empty<object>();

            using var input = new MockedIterator(rows);
            using var iterator = CreateIterator(input, 1);
            AssertEmpty(iterator);
        }

        [Fact]
        public void Iterators_TopWithTies_ReturnsEmpty_IfLimitIsZero()
        {
            var rows = new object[] { 1 };

            using var input = new MockedIterator(rows);
            using var iterator = CreateIterator(input, 0);
            AssertEmpty(iterator);
        }

        [Fact]
        public void Iterators_TopWithTies_ReturnsAllRows_IfLimitIsLarger()
        {
            var rows = new object[] { 1, 2, 3 };
            var expected = rows;
            var limit = expected.Length + 1;

            using var input = new MockedIterator(rows);
            using var iterator = CreateIterator(input, limit);
            AssertProduces(iterator, expected);
        }

        [Fact]
        public void Iterators_TopWithTies_LimitsRows_WhenNoTies()
        {
            var rows = new object[] { 1, 2, 3 };
            var expected = new object[] { 1, 2 };
            var limit = expected.Length;

            using var input = new MockedIterator(rows);
            using var iterator = CreateIterator(input, limit);
            AssertProduces(iterator, expected);
        }

        [Fact]
        public void Iterators_TopWithTies_LimitsRows_AndBreaksTies()
        {
            var rows = new object[] { 1, 2, 3, 3, 4 };
            var expected = new object[] { 1, 2, 3, 3 };
            const int limit = 3;

            using var input = new MockedIterator(rows);
            using var iterator = CreateIterator(input, limit);
            AssertProduces(iterator, expected);
        }

        [Fact]
        public void Iterators_TopWithTies_LimitsRows_AndStopsWhenLastRowIsInTie()
        {
            var rows = new object[] { 1, 2, 3, 3 };
            var expected = new object[] { 1, 2, 3, 3 };
            const int limit = 3;

            using var input = new MockedIterator(rows);
            using var iterator = CreateIterator(input, limit);
            AssertProduces(iterator, expected);
        }
    }
}