using NQuery.Iterators;

namespace NQuery.Tests.Iterators
{
    public class InnerNestedLoopsIteratorTests : IteratorTests
    {
        [Fact]
        public void Iterators_InnerNestedLoops_ForwardsProperly()
        {
            var leftRows = new object[] { 1, 2 };
            var rightRows = new object[] { 3, 4, 5 };
            var expected = new object[,]
            {
                {1, 3},
                {1, 4},
                {1, 5},
                {2, 3},
                {2, 4},
                {2, 5}
            };

            using var left = new MockedIterator(leftRows);
            using var right = new MockedIterator(rightRows);
            using (var iterator = new InnerNestedLoopsIterator(left, right, () => true, () => false))
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
            Assert.Equal(12, right.TotalReadCount);
            Assert.Equal(1, right.DisposalCount);
        }

        [Fact]
        public void Iterators_InnerNestedLoops_NoReadsOnRight_WhenPassthruIsTrue()
        {
            var leftRows = new object[] { 1, 2, 3, 4, 5 };
            var rightRows = new object[,]
            {
                {1, "One" },
                {2, "Two" },
                {3, "Three" }
            };
            var expected = new object[,]
            {
                {1, 1, "One"},
                {2, 3, "Three"},
                {3, 3, "Three"},
                {4, 3, "Three"},
            };

            using var left = new MockedIterator(leftRows);
            using var right = new MockedIterator(rightRows);
            var iteratorPredicate = new IteratorPredicate(() => Equals(left.RowBuffer[0], right.RowBuffer[0]));
            var passthru = new IteratorPredicate(() => (int)left.RowBuffer[0] % 2 == 0);

            using (var iterator = new InnerNestedLoopsIterator(left, right, iteratorPredicate, passthru))
            {
                AssertProduces(iterator, expected);
            }

            Assert.Equal(1, left.TotalOpenCount);
            Assert.Equal(5, left.TotalReadCount);
            Assert.Equal(1, left.DisposalCount);

            Assert.Equal(3, right.TotalOpenCount);
            Assert.Equal(9, right.TotalReadCount);
            Assert.Equal(1, right.DisposalCount);
        }

        [Fact]
        public void Iterators_InnerNestedLoops_ReturnsEmpty_WhenLeftAndRightIsEmpty()
        {
            var rows = Array.Empty<object>();
            var rightRows = new object[] { 1, 2, 3 };

            using var left = new MockedIterator(rows);
            using var right = new MockedIterator(rightRows);
            var iteratorPredicate = new IteratorPredicate(() => true);
            var passthru = new IteratorPredicate(() => false);

            using var iterator = new InnerNestedLoopsIterator(left, right, iteratorPredicate, passthru);
            AssertEmpty(iterator);
        }

        [Fact]
        public void Iterators_InnerNestedLoops_ReturnsEmpty_WhenLeftIsEmpty()
        {
            var leftRows = Array.Empty<object>();
            var rightRows = Array.Empty<object>();

            using var left = new MockedIterator(leftRows);
            using var right = new MockedIterator(rightRows);
            var iteratorPredicate = new IteratorPredicate(() => true);
            var passthru = new IteratorPredicate(() => false);

            using var iterator = new InnerNestedLoopsIterator(left, right, iteratorPredicate, passthru);
            AssertEmpty(iterator);
        }

        [Fact]
        public void Iterators_InnerNestedLoops_ReturnsEmpty_WhenRightIsEmpty()
        {
            var leftRows = new object[] { 1, 2, 3 };
            var rightRows = Array.Empty<object>();

            using var left = new MockedIterator(leftRows);
            using var right = new MockedIterator(rightRows);
            var iteratorPredicate = new IteratorPredicate(() => true);
            var passthru = new IteratorPredicate(() => false);

            using var iterator = new InnerNestedLoopsIterator(left, right, iteratorPredicate, passthru);
            AssertEmpty(iterator);
        }

        [Fact]
        public void Iterators_InnerNestedLoops_ReturnsEmpty_WhenRightIsEmpty_UnlessPassthruIsTrue()
        {
            var leftRows = new object[] { 1, 2, 3 };
            var rightRows = Array.Empty<object>();
            var expected = new object[,]
            {
                {2, null}
            };

            using var left = new MockedIterator(leftRows);
            using var right = new MockedIterator(rightRows);
            var iteratorPredicate = new IteratorPredicate(() => true);
            var passthru = new IteratorPredicate(() => Equals(left.RowBuffer[0], 2));

            using var iterator = new InnerNestedLoopsIterator(left, right, iteratorPredicate, passthru);
            AssertProduces(iterator, expected);
        }

        [Fact]
        public void Iterators_InnerNestedLoops_MatchesRows()
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

            var rightRows = new object[,]
            {
                {2, "2-Right"},
                {3, "3-Right-A"},
                {3, "3-Right-B"},
                {5, "5-Right"}
            };

            var expected = new object[,]
            {
                {2, "2-Left-A", 2, "2-Right" },
                {2, "2-Left-B", 2, "2-Right" },
                {3, "3-Left", 3, "3-Right-A" },
                {3, "3-Left", 3, "3-Right-B" },
                {5, "5-Left", 5, "5-Right" }
            };

            using var left = new MockedIterator(leftRows);
            using var right = new MockedIterator(rightRows);
            var iteratorPredicate = new IteratorPredicate(() => Equals(left.RowBuffer[0], right.RowBuffer[0]));
            var passthru = new IteratorPredicate(() => false);

            using var iterator = new InnerNestedLoopsIterator(left, right, iteratorPredicate, passthru);
            AssertProduces(iterator, expected);
        }
    }
}