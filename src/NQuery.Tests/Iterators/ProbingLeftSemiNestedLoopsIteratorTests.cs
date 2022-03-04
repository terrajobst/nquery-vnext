using NQuery.Iterators;

namespace NQuery.Tests.Iterators
{
    public class ProbingLeftSemiNestedLoopsIteratorTests : IteratorTests
    {
        [Fact]
        public void Iterators_ProbingLeftSemiNestedLoops_ForwardsProperly()
        {
            var leftRows = new object[] { 1, 2 };
            var rightRows = new object[] { 3, 4 };
            var expected = new object[,]
            {
                {1, true},
                {2, true}
            };

            using (var left = new MockedIterator(leftRows))
            using (var right = new MockedIterator(rightRows))
            {
                using (var iterator = new ProbingLeftSemiNestedLoopsIterator(left, right, () => true))
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
        }

        [Fact]
        public void Iterators_ProbingLeftSemiNestedLoops_ReturnsEmpty_WhenLeftAndRightIsEmpty()
        {
            var leftRows = new object[0];
            var rightRows = new object[] { 1, 2, 3 };

            using (var left = new MockedIterator(leftRows))
            using (var right = new MockedIterator(rightRows))
            {
                var iteratorPredicate = new IteratorPredicate(() => true);

                using (var iterator = new ProbingLeftSemiNestedLoopsIterator(left, right, iteratorPredicate))
                {
                    AssertEmpty(iterator);
                }
            }
        }

        [Fact]
        public void Iterators_ProbingLeftSemiNestedLoops_ReturnsEmpty_WhenLeftIsEmpty()
        {
            var leftRows = new object[0];
            var rightRows = new object[0];

            using (var left = new MockedIterator(leftRows))
            using (var right = new MockedIterator(rightRows))
            {
                var iteratorPredicate = new IteratorPredicate(() => true);

                using (var iterator = new ProbingLeftSemiNestedLoopsIterator(left, right, iteratorPredicate))
                {
                    AssertEmpty(iterator);
                }
            }
        }

        [Fact]
        public void Iterators_ProbingLeftSemiNestedLoops_ReturnsLeft_WhenRightIsEmpty()
        {
            var leftRows = new object[] { 1, 2, 3 };
            var rightRows = new object[0];
            var expected = new object[,]
            {
                {1, false},
                {2, false},
                {3, false}
            };

            using (var left = new MockedIterator(leftRows))
            using (var right = new MockedIterator(rightRows))
            {
                var iteratorPredicate = new IteratorPredicate(() => true);

                using (var iterator = new ProbingLeftSemiNestedLoopsIterator(left, right, iteratorPredicate))
                {
                    AssertProduces(iterator, expected);
                }
            }
        }

        [Fact]
        public void Iterators_ProbingLeftSemiNestedLoops_MatchesRows()
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
                {1, "1-Left", false},
                {2, "2-Left-A", true},
                {2, "2-Left-B", true},
                {3, "3-Left", true},
                {4, "4-Left", false},
                {5, "5-Left", true}
            };

            using (var left = new MockedIterator(leftRows))
            using (var right = new MockedIterator(rightRows))
            {
                var iteratorPredicate = new IteratorPredicate(() => Equals(left.RowBuffer[0], right.RowBuffer[0]));

                using (var iterator = new ProbingLeftSemiNestedLoopsIterator(left, right, iteratorPredicate))
                {
                    AssertProduces(iterator, expected);
                }
            }
        }
    }
}