using NQuery.Iterators;

using Xunit;

namespace NQuery.Tests.Iterators
{
    public class TopIteratorTests : IteratorTests
    {
        [Fact]
        public void Iterators_Top_ForwardsProperly()
        {
            var rows = new object[] { 1, 2 };
            var expected = new object[] {1, 2};

            using (var input = new MockedIterator(rows))
            {
                using (var iterator = new TopIterator(input, int.MaxValue))
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
        }

        [Fact]
        public void Iterators_Top_ReturnsEmpty_IfInputIsEmpty()
        {
            var rows = new object[0];

            using (var input = new MockedIterator(rows))
            using (var iterator = new TopIterator(input, 1))
            {
                AssertEmpty(iterator);
            }
        }

        [Fact]
        public void Iterators_Top_ReturnsEmpty_IfLimitIsZero()
        {
            var rows = new object[] {1};

            using (var input = new MockedIterator(rows))
            using (var iterator = new TopIterator(input, 0))
            {
                AssertEmpty(iterator);
            }
        }

        [Fact]
        public void Iterators_Top_ReturnsAllRows_IfLimitIsLarger()
        {
            var rows = new object[] {1, 2, 3};
            var expected = rows;
            var limit = rows.Length + 1;

            using (var input = new MockedIterator(rows))
            using (var iterator = new TopIterator(input, limit))
            {
                AssertProduces(iterator, expected);
            }
        }

        [Fact]
        public void Iterators_Top_LimitsRows()
        {
            var rows = new object[] {1, 2, 3};
            var expected = new object[] {1, 2};
            var limit = expected.Length;

            using (var input = new MockedIterator(rows))
            using (var iterator = new TopIterator(input, limit))
            {
                AssertProduces(iterator, expected);
            }
        }
    }
}