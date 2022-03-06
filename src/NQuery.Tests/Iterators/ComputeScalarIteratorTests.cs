using NQuery.Iterators;

namespace NQuery.Tests.Iterators
{
    public class ComputeScalarIteratorTests : IteratorTests
    {
        [Fact]
        public void Iterators_ComputeScalar_ForwardsProperly()
        {
            var values = Enumerable.Empty<IteratorFunction>();

            var rows = new object[] { 1, 2 };
            var expected = rows;

            using (var input = new MockedIterator(rows))
            {
                using (var iterator = new ComputeScalarIterator(input, values))
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
        public void Iterators_ComputeScalar_ReturnsEmpty_IfInputIsEmpty()
        {
            var rows = Array.Empty<object>();
            var values = new IteratorFunction[]
            {
                () => 1
            };

            using (var input = new MockedIterator(rows))
            using (var iterator = new ComputeScalarIterator(input, values))
            {
                AssertEmpty(iterator);
            }
        }

        [Fact]
        public void Iterators_ComputeScalar_ComputesValues()
        {
            var rows = new object[]
            {
                4, 6, 8
            };

            var expected = new object[,]
            {
                {4, 12},
                {6, 18},
                {8, 24}
            };

            using (var input = new MockedIterator(rows))
            {
                var values = new IteratorFunction[]
                {
                    () => (int)input.RowBuffer[0] * 3
                };

                using (var iterator = new ComputeScalarIterator(input, values))
                {
                    AssertProduces(iterator, expected);
                }
            }
        }
    }
}