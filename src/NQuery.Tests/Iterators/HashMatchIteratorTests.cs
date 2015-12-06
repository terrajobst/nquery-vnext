using System;

using NQuery.Binding;
using NQuery.Iterators;

using Xunit;

namespace NQuery.Tests.Iterators
{
    public class HashMatchIteratorTests : IteratorTests
    {
        [Theory]
        [InlineData(BoundHashMatchOperator.Inner)]
        [InlineData(BoundHashMatchOperator.LeftOuter)]
        [InlineData(BoundHashMatchOperator.RightOuter)]
        [InlineData(BoundHashMatchOperator.FullOuter)]
        internal void Iterators_HashMatch_ForwardsProperly(BoundHashMatchOperator logicalOperator)
        {
            var buildRows = new object[] {1, 2};
            var probeRows = new object[] {2, 3};

            using (var build = new MockedIterator(buildRows))
            using (var probe = new MockedIterator(probeRows))
            {
                const int passCount = 2;

                var rowBuffer = new HashMatchRowBuffer(build.RowBuffer.Count, probe.RowBuffer.Count);
                var remainder = new IteratorPredicate(() => true);

                using (var iterator = new HashMatchIterator(logicalOperator, build, probe, 0, 0, remainder, rowBuffer))
                {
                    for (var i = 0; i < passCount; i++)
                    {
                        iterator.Open();

                        Assert.True(iterator.Read());
                        Assert.Equal(2, iterator.RowBuffer[0]);
                        Assert.Equal(2, iterator.RowBuffer[1]);

                        if (logicalOperator == BoundHashMatchOperator.RightOuter ||
                            logicalOperator == BoundHashMatchOperator.FullOuter)
                        {
                            Assert.True(iterator.Read());
                            Assert.Equal(null, iterator.RowBuffer[0]);
                            Assert.Equal(3, iterator.RowBuffer[1]);
                        }

                        if (logicalOperator == BoundHashMatchOperator.LeftOuter ||
                            logicalOperator == BoundHashMatchOperator.FullOuter)
                        {
                            Assert.True(iterator.Read());
                            Assert.Equal(1, iterator.RowBuffer[0]);
                            Assert.Equal(null, iterator.RowBuffer[1]);
                        }

                        Assert.False(iterator.Read());
                    }
                }

                var inputs = new[] {build, probe};

                foreach (var input in inputs)
                {
                    Assert.Equal(passCount, input.TotalOpenCount);
                    Assert.Equal(passCount * 2, input.TotalReadCount);
                    Assert.Equal(1, input.DisposalCount);
                }
            }
        }

        [Theory]
        [InlineData(BoundHashMatchOperator.Inner)]
        [InlineData(BoundHashMatchOperator.LeftOuter)]
        internal void Iterators_HashMatch_ReturnsEmpty_IfBuildIsEmpty(BoundHashMatchOperator logicalOperator)
        {
            var buildRows = new object[0];
            var probeRows = new object[] { 2, 3 };

            using (var build = new MockedIterator(buildRows))
            using (var probe = new MockedIterator(probeRows))
            {
                var rowBuffer = new HashMatchRowBuffer(build.RowBuffer.Count, probe.RowBuffer.Count);
                var remainder = new IteratorPredicate(() => true);

                using (var iterator = new HashMatchIterator(logicalOperator, build, probe, 0, 0, remainder, rowBuffer))
                {
                    AssertEmpty(iterator);
                }
            }
        }

        [Theory]
        [InlineData(BoundHashMatchOperator.RightOuter)]
        [InlineData(BoundHashMatchOperator.FullOuter)]
        internal void Iterators_HashMatch_ReturnsProbe_IfBuildIsEmpty(BoundHashMatchOperator logicalOperator)
        {
            var buildRows = new object[0];
            var probeRows = new object[] { 2, 3 };
            var expected = new object[,]
            {
                {null, 2},
                {null, 3}
            };

            using (var build = new MockedIterator(buildRows))
            using (var probe = new MockedIterator(probeRows))
            {
                var rowBuffer = new HashMatchRowBuffer(build.RowBuffer.Count, probe.RowBuffer.Count);
                var remainder = new IteratorPredicate(() => true);

                using (var iterator = new HashMatchIterator(logicalOperator, build, probe, 0, 0, remainder, rowBuffer))
                {
                    AssertProduces(iterator, expected);
                }
            }
        }

        [Theory]
        [InlineData(BoundHashMatchOperator.Inner)]
        [InlineData(BoundHashMatchOperator.RightOuter)]
        internal void Iterators_HashMatch_ReturnsEmpty_IfProbeIsEmpty(BoundHashMatchOperator logicalOperator)
        {
            var buildRows = new object[] { 1, 2 };
            var probeRows = new object[0];

            using (var build = new MockedIterator(buildRows))
            using (var probe = new MockedIterator(probeRows))
            {
                var rowBuffer = new HashMatchRowBuffer(build.RowBuffer.Count, probe.RowBuffer.Count);
                var remainder = new IteratorPredicate(() => true);

                using (var iterator = new HashMatchIterator(logicalOperator, build, probe, 0, 0, remainder, rowBuffer))
                {
                    AssertEmpty(iterator);
                }
            }
        }

        [Theory]
        [InlineData(BoundHashMatchOperator.LeftOuter)]
        [InlineData(BoundHashMatchOperator.FullOuter)]
        internal void Iterators_HashMatch_ReturnsBuild_IfProbeIsEmpty(BoundHashMatchOperator logicalOperator)
        {
            var buildRows = new object[] { 1, 2 };
            var probeRows = new object[0];
            var expected = new object[,]
            {
                {1, null},
                {2, null}
            };

            using (var build = new MockedIterator(buildRows))
            using (var probe = new MockedIterator(probeRows))
            {
                var rowBuffer = new HashMatchRowBuffer(build.RowBuffer.Count, probe.RowBuffer.Count);
                var remainder = new IteratorPredicate(() => true);

                using (var iterator = new HashMatchIterator(logicalOperator, build, probe, 0, 0, remainder, rowBuffer))
                {
                    AssertProduces(iterator, expected);
                }
            }
        }

        [Theory]
        [InlineData(BoundHashMatchOperator.Inner)]
        [InlineData(BoundHashMatchOperator.LeftOuter)]
        [InlineData(BoundHashMatchOperator.RightOuter)]
        [InlineData(BoundHashMatchOperator.FullOuter)]
        internal void Iterators_HashMatch_DoesNotMatchNulls(BoundHashMatchOperator logicalOperator)
        {
            var buildRows = new object[,]
            {
                {null, "Foo"}
            };
            var probeRows = new object[,]
            {
                {null, "Bar"}
            };

            using (var build = new MockedIterator(buildRows))
            using (var probe = new MockedIterator(probeRows))
            {
                var rowBuffer = new HashMatchRowBuffer(build.RowBuffer.Count, probe.RowBuffer.Count);
                var remainder = new IteratorPredicate(() => true);

                using (var iterator = new HashMatchIterator(logicalOperator, build, probe, 0, 0, remainder, rowBuffer))
                {
                    iterator.Open();

                    if (logicalOperator == BoundHashMatchOperator.RightOuter ||
                        logicalOperator == BoundHashMatchOperator.FullOuter)
                    {
                        Assert.True(iterator.Read());
                        Assert.Equal(null, iterator.RowBuffer[0]);
                        Assert.Equal(null, iterator.RowBuffer[1]);
                        Assert.Equal(null, iterator.RowBuffer[2]);
                        Assert.Equal("Bar", iterator.RowBuffer[3]);
                    }

                    if (logicalOperator == BoundHashMatchOperator.LeftOuter ||
                        logicalOperator == BoundHashMatchOperator.FullOuter)
                    {
                        Assert.True(iterator.Read());
                        Assert.Equal(null, iterator.RowBuffer[0]);
                        Assert.Equal("Foo", iterator.RowBuffer[1]);
                        Assert.Equal(null, iterator.RowBuffer[2]);
                        Assert.Equal(null, iterator.RowBuffer[3]);
                    }

                    Assert.False(iterator.Read());
                }
            }
        }

        [Theory]
        [InlineData(BoundHashMatchOperator.Inner)]
        [InlineData(BoundHashMatchOperator.LeftOuter)]
        [InlineData(BoundHashMatchOperator.RightOuter)]
        [InlineData(BoundHashMatchOperator.FullOuter)]
        internal void Iterators_HashMatch_MatchesDuplicates(BoundHashMatchOperator logicalOperator)
        {
            var buildRows = new object[] {1, 2, 3};
            var probeRows = new object[,]
            {
                {1, "First"},
                {2, "Second1"},
                {2, "Second2"},
                {3, "Third"}
            };
            var expected = new object[,]
            {
                {1, 1, "First"},
                {2, 2, "Second1"},
                {2, 2, "Second2"},
                {3, 3, "Third"}
            };

            using (var build = new MockedIterator(buildRows))
            using (var probe = new MockedIterator(probeRows))
            {
                var rowBuffer = new HashMatchRowBuffer(build.RowBuffer.Count, probe.RowBuffer.Count);
                var remainder = new IteratorPredicate(() => true);

                using (var iterator = new HashMatchIterator(logicalOperator, build, probe, 0, 0, remainder, rowBuffer))
                {
                    AssertProduces(iterator, expected);
                }
            }
        }

        [Theory]
        [InlineData(BoundHashMatchOperator.Inner)]
        [InlineData(BoundHashMatchOperator.LeftOuter)]
        [InlineData(BoundHashMatchOperator.RightOuter)]
        [InlineData(BoundHashMatchOperator.FullOuter)]
        internal void Iterators_HashMatch_MatchesWithRemainder(BoundHashMatchOperator logicalOperator)
        {
            var buildRows = new object[,]
            {
                {1, 1},
                {1, 2},
                {2, 1},
                {2, 2},
                {3, 1}
            };

            var probeRows = new object[,]
            {
                {0, 0, "Unmatched1" },
                {1, 1, "Project1-Task-1"},
                {1, 2, "Project1-Task-2"},
                {1, 3, "Unmatched2"},
                {2, 1, "Project2-Task-1"},
            };

            using (var build = new MockedIterator(buildRows))
            using (var probe = new MockedIterator(probeRows))
            {
                // Layout:
                // 0               | 1            | 2               | 3            | 4
                // ----------------+--------------+-----------------+--------------+---------------
                // build.ProjectId | build.TaskId | probe.ProjectId | probe.TaskId | probe.TaskName

                var rowBuffer = new HashMatchRowBuffer(build.RowBuffer.Count, probe.RowBuffer.Count);
                var remainder = new IteratorPredicate(() => Equals(rowBuffer[1], rowBuffer[3]));

                using (var iterator = new HashMatchIterator(logicalOperator, build, probe, 0, 0, remainder, rowBuffer))
                {
                    iterator.Open();

                    if (logicalOperator == BoundHashMatchOperator.RightOuter ||
                        logicalOperator == BoundHashMatchOperator.FullOuter)
                    {
                        Assert.True(iterator.Read());
                        Assert.Equal(null, iterator.RowBuffer[0]);
                        Assert.Equal(null, iterator.RowBuffer[1]);
                        Assert.Equal(0, iterator.RowBuffer[2]);
                        Assert.Equal(0, iterator.RowBuffer[3]);
                        Assert.Equal("Unmatched1", iterator.RowBuffer[4]);
                    }

                    Assert.True(iterator.Read());
                    Assert.Equal(1, iterator.RowBuffer[0]);
                    Assert.Equal(1, iterator.RowBuffer[1]);
                    Assert.Equal(1, iterator.RowBuffer[2]);
                    Assert.Equal(1, iterator.RowBuffer[3]);
                    Assert.Equal("Project1-Task-1", iterator.RowBuffer[4]);

                    Assert.True(iterator.Read());
                    Assert.Equal(1, iterator.RowBuffer[0]);
                    Assert.Equal(2, iterator.RowBuffer[1]);
                    Assert.Equal(1, iterator.RowBuffer[2]);
                    Assert.Equal(2, iterator.RowBuffer[3]);
                    Assert.Equal("Project1-Task-2", iterator.RowBuffer[4]);

                    if (logicalOperator == BoundHashMatchOperator.RightOuter ||
                        logicalOperator == BoundHashMatchOperator.FullOuter)
                    {
                        Assert.True(iterator.Read());
                        Assert.Equal(null, iterator.RowBuffer[0]);
                        Assert.Equal(null, iterator.RowBuffer[1]);
                        Assert.Equal(1, iterator.RowBuffer[2]);
                        Assert.Equal(3, iterator.RowBuffer[3]);
                        Assert.Equal("Unmatched2", iterator.RowBuffer[4]);
                    }

                    Assert.True(iterator.Read());
                    Assert.Equal(2, iterator.RowBuffer[0]);
                    Assert.Equal(1, iterator.RowBuffer[1]);
                    Assert.Equal(2, iterator.RowBuffer[2]);
                    Assert.Equal(1, iterator.RowBuffer[3]);
                    Assert.Equal("Project2-Task-1", iterator.RowBuffer[4]);

                    if (logicalOperator == BoundHashMatchOperator.LeftOuter ||
                        logicalOperator == BoundHashMatchOperator.FullOuter)
                    {
                        Assert.True(iterator.Read());
                        Assert.Equal(2, iterator.RowBuffer[0]);
                        Assert.Equal(2, iterator.RowBuffer[1]);
                        Assert.Equal(null, iterator.RowBuffer[2]);
                        Assert.Equal(null, iterator.RowBuffer[3]);
                        Assert.Equal(null, iterator.RowBuffer[4]);

                        Assert.True(iterator.Read());
                        Assert.Equal(3, iterator.RowBuffer[0]);
                        Assert.Equal(1, iterator.RowBuffer[1]);
                        Assert.Equal(null, iterator.RowBuffer[2]);
                        Assert.Equal(null, iterator.RowBuffer[3]);
                        Assert.Equal(null, iterator.RowBuffer[4]);
                    }

                    Assert.False(iterator.Read());
                }
            }
        }
    }
}