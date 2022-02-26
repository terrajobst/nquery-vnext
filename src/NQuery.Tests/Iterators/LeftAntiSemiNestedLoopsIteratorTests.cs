using NQuery.Iterators;

namespace NQuery.Tests.Iterators
{
    public class LeftAntiSemiNestedLoopsIteratorTests : IteratorTests
    {
        [Fact]
        public void Iterators_LeftAntiSemiNestedLoops_ForwardsProperly()
        {
            var leftRows = new object[] {1, 2};
            var rightRows = new object[] {3, 4};

            using (var left = new MockedIterator(leftRows))
            using (var right = new MockedIterator(rightRows))
            {
                using (var iterator = new LeftAntiSemiNestedLoopsIterator(left, right, () => true, () => false))
                {
                    for (var i = 0; i < 2; i++)
                    {
                        AssertEmpty(iterator);
                    }
                }

                Assert.Equal(2, left.TotalOpenCount);
                Assert.Equal(4, left.TotalReadCount);
                Assert.Equal(1, left.DisposalCount);

                Assert.Equal(4, right.TotalOpenCount);
                Assert.Equal(4, right.TotalReadCount);
                Assert.Equal(1, right.DisposalCount);
            }
        }

        [Fact]
        public void Iterators_LeftAntiSemiNestedLoops_NoReadsOnRight_WhenPassthruIsTrue()
        {
            var leftRows = new object[] {1, 2, 3, 4, 5};
            var rightRows = new object[] {1, 2, 3};
            var expected = new object[] {2, 4, 5};

            using (var left = new MockedIterator(leftRows))
            using (var right = new MockedIterator(rightRows))
            {
                var iteratorPredicate = new IteratorPredicate(() => Equals(left.RowBuffer[0], right.RowBuffer[0]));
                var passthru = new IteratorPredicate(() => (int)left.RowBuffer[0] % 2 == 0);

                using (var iterator = new LeftAntiSemiNestedLoopsIterator(left, right, iteratorPredicate, passthru))
                {
                    AssertProduces(iterator, expected);
                }

                Assert.Equal(1, left.TotalOpenCount);
                Assert.Equal(5, left.TotalReadCount);
                Assert.Equal(1, left.DisposalCount);

                Assert.Equal(3, right.TotalOpenCount);
                Assert.Equal(7, right.TotalReadCount);
                Assert.Equal(1, right.DisposalCount);
            }
        }

        [Fact]
        public void Iterators_LeftAntiSemiNestedLoops_ReturnsEmpty_WhenLeftAndRightIsEmpty()
        {
            var leftRows = new object[0];
            var rightRows = new object[] {1, 2, 3};

            using (var left = new MockedIterator(leftRows))
            using (var right = new MockedIterator(rightRows))
            {
                var iteratorPredicate = new IteratorPredicate(() => true);
                var passthru = new IteratorPredicate(() => false);

                using (var iterator = new LeftAntiSemiNestedLoopsIterator(left, right, iteratorPredicate, passthru))
                {
                    AssertEmpty(iterator);
                }
            }
        }

        [Fact]
        public void Iterators_LeftAntiSemiNestedLoops_ReturnsEmpty_WhenLeftIsEmpty()
        {
            var leftRows = new object[0];
            var rightRows = new object[0];

            using (var left = new MockedIterator(leftRows))
            using (var right = new MockedIterator(rightRows))
            {
                var iteratorPredicate = new IteratorPredicate(() => true);
                var passthru = new IteratorPredicate(() => false);

                using (var iterator = new LeftAntiSemiNestedLoopsIterator(left, right, iteratorPredicate, passthru))
                {
                    AssertEmpty(iterator);
                }
            }
        }

        [Fact]
        public void Iterators_LeftAntiSemiNestedLoops_ReturnsEmpty_WhenRightIsNonEmpty()
        {
            var leftRows = new object[] {1, 2, 3};
            var rightRows = new object[] {1};

            using (var left = new MockedIterator(leftRows))
            using (var right = new MockedIterator(rightRows))
            {
                var iteratorPredicate = new IteratorPredicate(() => true);
                var passthru = new IteratorPredicate(() => false);

                using (var iterator = new LeftAntiSemiNestedLoopsIterator(left, right, iteratorPredicate, passthru))
                {
                    AssertEmpty(iterator);
                }
            }
        }

        [Fact]
        public void Iterators_LeftAntiSemiNestedLoops_ReturnsLeft_WhenRightEmpty()
        {
            var leftRows = new object[] {1, 2, 3};
            var rightRows = new object[0];
            var expected = leftRows;

            using (var left = new MockedIterator(leftRows))
            using (var right = new MockedIterator(rightRows))
            {
                var iteratorPredicate = new IteratorPredicate(() => true);
                var passthru = new IteratorPredicate(() => false);

                using (var iterator = new LeftAntiSemiNestedLoopsIterator(left, right, iteratorPredicate, passthru))
                {
                    AssertProduces(iterator, expected);
                }
            }
        }

        [Fact]
        public void Iterators_LeftAntiSemiNestedLoops_ReturnsLeft_WhenRightIsNonEmpty_UnlessPassthruIsTrue()
        {
            var leftRows = new object[] {1, 2, 3};
            var rightRows = new object[] {1};
            var expected = new object[] {2};

            using (var left = new MockedIterator(leftRows))
            using (var right = new MockedIterator(rightRows))
            {
                var iteratorPredicate = new IteratorPredicate(() => true);
                var passthru = new IteratorPredicate(() => Equals(left.RowBuffer[0], 2));

                using (var iterator = new LeftAntiSemiNestedLoopsIterator(left, right, iteratorPredicate, passthru))
                {
                    AssertProduces(iterator, expected);
                }
            }
        }

        [Fact]
        public void Iterators_LeftAntiSemiNestedLoops_MatchesRows()
        {
            var leftRows = new object[,]
            {
                {1, "1-Left"},
                {2, "2-Left-A"},
                {2, "2-Left-B"},
                {3, "3-Left-A"},
                {3, "3-Left-B"},
                {4, "4-Left"},
                {5, "5-Left"}
            };

            var rightRows = new object[] {1, 3, 5};

            var expected = new object[,]
            {
                {2, "2-Left-A"},
                {2, "2-Left-B"},
                {4, "4-Left"}
            };

            using (var left = new MockedIterator(leftRows))
            using (var right = new MockedIterator(rightRows))
            {
                var iteratorPredicate = new IteratorPredicate(() => Equals(left.RowBuffer[0], right.RowBuffer[0]));
                var passthru = new IteratorPredicate(() => false);

                using (var iterator = new LeftAntiSemiNestedLoopsIterator(left, right, iteratorPredicate, passthru))
                {
                    AssertProduces(iterator, expected);
                }
            }
        }
    }
}