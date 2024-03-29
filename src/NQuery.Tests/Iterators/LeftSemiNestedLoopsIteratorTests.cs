using NQuery.Iterators;

namespace NQuery.Tests.Iterators
{
    public class LeftSemiNestedLoopsIteratorTests : IteratorTests
    {
        [Fact]
        public void Iterators_LeftSemiNestedLoops_ForwardsProperly()
        {
            var leftRows = new object[] { 1, 2 };
            var rightRows = new object[] { 3, 4, 5 };
            var expected = leftRows;

            using var left = new MockedIterator(leftRows);
            using var right = new MockedIterator(rightRows);
            using (var iterator = new LeftSemiNestedLoopsIterator(left, right, () => true, () => false))
            {
                for (var i = 0; i < 2; i++)
                {
                    AssertProduces(iterator, expected);
                }
            }

            Assert.Equal(2, left.TotalOpenCount);
            Assert.Equal(4, left.TotalReadCount);
            Assert.Equal(1, left.DisposalCount);

            Assert.Equal(4, right.TotalOpenCount);
            Assert.Equal(4, right.TotalReadCount);
            Assert.Equal(1, right.DisposalCount);
        }

        [Fact]
        public void Iterators_LeftSemiNestedLoops_NoReadsOnRight_WhenPassthruIsTrue()
        {
            var leftRows = new object[] { 1, 2, 3, 4, 5 };
            var rightRows = new object[] { 1, 2, 3 };
            var expectedRows = new object[] { 1, 2, 3, 4 };

            using var left = new MockedIterator(leftRows);
            using var right = new MockedIterator(rightRows);
            var iteratorPredicate = new IteratorPredicate(() => Equals(left.RowBuffer[0], right.RowBuffer[0]));
            var passthru = new IteratorPredicate(() => (int)left.RowBuffer[0] % 2 == 0);

            using (var iterator = new LeftSemiNestedLoopsIterator(left, right, iteratorPredicate, passthru))
            {
                AssertProduces(iterator, expectedRows);
            }

            Assert.Equal(1, left.TotalOpenCount);
            Assert.Equal(5, left.TotalReadCount);
            Assert.Equal(1, left.DisposalCount);

            Assert.Equal(3, right.TotalOpenCount);
            Assert.Equal(7, right.TotalReadCount);
            Assert.Equal(1, right.DisposalCount);
        }

        [Fact]
        public void Iterators_LeftSemiNestedLoops_ReturnsEmpty_WhenLeftAndRightIsEmpty()
        {
            var leftRows = Array.Empty<object>();
            var rightRows = new object[] { 1, 2, 3 };

            using var left = new MockedIterator(leftRows);
            using var right = new MockedIterator(rightRows);
            var iteratorPredicate = new IteratorPredicate(() => true);
            var passthru = new IteratorPredicate(() => false);

            using var iterator = new LeftSemiNestedLoopsIterator(left, right, iteratorPredicate, passthru);
            AssertEmpty(iterator);
        }

        [Fact]
        public void Iterators_LeftSemiNestedLoops_ReturnsEmpty_WhenLeftIsEmpty()
        {
            var leftRows = Array.Empty<object>();
            var rightRows = Array.Empty<object>();

            using var left = new MockedIterator(leftRows);
            using var right = new MockedIterator(rightRows);
            var iteratorPredicate = new IteratorPredicate(() => true);
            var passthru = new IteratorPredicate(() => false);

            using var iterator = new LeftSemiNestedLoopsIterator(left, right, iteratorPredicate, passthru);
            AssertEmpty(iterator);
        }

        [Fact]
        public void Iterators_LeftSemiNestedLoops_ReturnsEmpty_WhenRightIsEmpty()
        {
            var leftRows = new object[] { 1, 2, 3 };
            var rightRows = Array.Empty<object>();

            using var left = new MockedIterator(leftRows);
            using var right = new MockedIterator(rightRows);
            var iteratorPredicate = new IteratorPredicate(() => true);
            var passthru = new IteratorPredicate(() => false);

            using var iterator = new LeftSemiNestedLoopsIterator(left, right, iteratorPredicate, passthru);
            AssertEmpty(iterator);
        }

        [Fact]
        public void Iterators_LeftSemiNestedLoops_ReturnsEmpty_WhenRightIsEmpty_UnlessPassthruIsTrue()
        {
            var leftRows = new object[] { 1, 2, 3 };
            var rightRows = Array.Empty<object>();
            var expected = new object[] { 2 };

            using var left = new MockedIterator(leftRows);
            using var right = new MockedIterator(rightRows);
            var iteratorPredicate = new IteratorPredicate(() => true);
            var passthru = new IteratorPredicate(() => Equals(left.RowBuffer[0], 2));

            using var iterator = new LeftSemiNestedLoopsIterator(left, right, iteratorPredicate, passthru);
            AssertProduces(iterator, expected);
        }

        [Fact]
        public void Iterators_LeftSemiNestedLoops_MatchesRows()
        {
            var leftRows = new object[,]
            {
                {1, "1-Left"},
                {2, "2-Left-A"},
                {2, "2-Left-B"},
                {3, "3-Left"},
                {4, "4-Left"},
                {5, "5-Left"}
            };

            var rightRows = new object[] { 2, 3, 3, 5 };

            var expected = new object[,]
            {
                {2, "2-Left-A"},
                {2, "2-Left-B"},
                {3, "3-Left"},
                {5, "5-Left"}
            };

            using var left = new MockedIterator(leftRows);
            using var right = new MockedIterator(rightRows);
            var iteratorPredicate = new IteratorPredicate(() => Equals(left.RowBuffer[0], right.RowBuffer[0]));
            var passthru = new IteratorPredicate(() => false);

            using var iterator = new LeftSemiNestedLoopsIterator(left, right, iteratorPredicate, passthru);
            AssertProduces(iterator, expected);
        }
    }
}