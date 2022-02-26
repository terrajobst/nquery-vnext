using NQuery.Iterators;

namespace NQuery.Tests.Iterators
{
    public class FilterIteratorTests : IteratorTests
    {
        [Fact]
        public void Iterators_Filter_ForwardsProperly()
        {
            var rows = new object[] { 1, 2 };
            var expected = rows;

            using (var input = new MockedIterator(rows))
            {
                using (var iterator = new FilterIterator(input, () => true))
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
        public void Iterators_Filter_ReturnsEmpty_IfInputIsEmpty()
        {
            var rows = new object[0];

            using (var input = new MockedIterator(rows))
            using (var iterator = new FilterIterator(input, () => true))
            {
                AssertEmpty(iterator);
            }
        }

        [Fact]
        public void Iterators_Filter_ReturnsEmpty_IfPredicateIsFalse()
        {
            using (var input = new ConstantIterator())
            using (var iterator = new FilterIterator(input, () => false))
            {
                AssertEmpty(iterator);
            }
        }

        [Fact]
        public void Iterators_Filter_Filters()
        {
            var rows = new object[] {1, 2, 3, 4};
            var expected = new object[] {2, 4};

            using (var input = new MockedIterator(rows))
            using (var iterator = new FilterIterator(input, () => (int)input.RowBuffer[0] % 2 == 0))
            {
                AssertProduces(iterator, expected);
            }
        }
    }
}