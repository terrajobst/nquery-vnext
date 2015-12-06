using System;
using System.Collections;
using System.Collections.Immutable;
using System.Linq;

using NQuery.Iterators;
using NQuery.Symbols.Aggregation;

using Xunit;

namespace NQuery.Tests.Iterators
{
    public class StreamAggregateIteratorTests : IteratorTests
    {
        [Fact]
        public void Iterators_StreamAggregate_ForwardsProperly()
        {
            var rows = new object[] {1, 2};
            var expected = new object[1, 0];

            using (var input = new MockedIterator(rows))
            {
                var groupEntries = Enumerable.Empty<RowBufferEntry>();
                var comparers = ImmutableArray<IComparer>.Empty;
                var aggregators = Enumerable.Empty<IAggregator>();
                var argumentFunctions = Enumerable.Empty<IteratorFunction>();

                using (var iterator = new StreamAggregateIterator(input, groupEntries, comparers, aggregators, argumentFunctions))
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
        public void Iterators_StreamAggregate_ReturnsEmpty_IfInputEmptyAndGrouped()
        {
            var rows = new object[0];

            using (var input = new MockedIterator(rows))
            {
                var groupEntries = new [] { new RowBufferEntry(input.RowBuffer, 0) };
                var comparers = ImmutableArray.Create<IComparer>(Comparer.Default);
                var aggregators = Enumerable.Empty<IAggregator>();
                var argumentFunctions = Enumerable.Empty<IteratorFunction>();

                using (var iterator = new StreamAggregateIterator(input, groupEntries, comparers, aggregators, argumentFunctions))
                {
                    AssertEmpty(iterator);
                }
            }
        }

        [Fact]
        public void Iterators_StreamAggregate_ReturnsSingleRow_IfInputEmptyAndNotGrouped()
        {
            var rows = new object[0];
            var expected = new object[1, 0];

            using (var input = new MockedIterator(rows))
            {
                var groupEntries = Enumerable.Empty<RowBufferEntry>();
                var comparers = ImmutableArray<IComparer>.Empty;
                var aggregators = Enumerable.Empty<IAggregator>();
                var argumentFunctions = Enumerable.Empty<IteratorFunction>();

                using (var iterator = new StreamAggregateIterator(input, groupEntries, comparers, aggregators, argumentFunctions))
                {
                    AssertProduces(iterator, expected);
                }
            }
        }

        [Fact]
        public void Iterators_StreamAggregate_ComputeAggregates_WhenNotGrouped()
        {
            var rows = new object[] {1, 2, 3};
            var expected = new object[,]
            {
                {3, 1}
            };

            using (var input = new MockedIterator(rows))
            {
                var groupEntries = Enumerable.Empty<RowBufferEntry>();
                var comparers = ImmutableArray<IComparer>.Empty;
                var aggregators = new []
                {
                    new MaxAggregateDefinition().CreateAggregatable(typeof(int)).CreateAggregator(),
                    new MinAggregateDefinition().CreateAggregatable(typeof(int)).CreateAggregator()
                };

                var function = new IteratorFunction(() => input.RowBuffer[0]);

                var argumentFunctions = new[] { function, function };

                using (var iterator = new StreamAggregateIterator(input, groupEntries, comparers, aggregators, argumentFunctions))
                {
                    AssertProduces(iterator, expected);
                }
            }
        }

        [Fact]
        public void Iterators_StreamAggregate_ComputeAggregates_WhenGrouped()
        {
            var rows = new object[,]
            {
                {"One", 1},
                {"One", 2},
                {"Two", 3}
            };

            var expected = new object[,]
            {
                {"One", 2, 1},
                {"Two", 3, 3}
            };

            using (var input = new MockedIterator(rows))
            {
                var groupEntries = new []
                {
                    new RowBufferEntry(input.RowBuffer, 0)
                };
                var comparers = ImmutableArray.Create<IComparer>(Comparer.Default);
                var aggregators = new []
                {
                    new MaxAggregateDefinition().CreateAggregatable(typeof(int)).CreateAggregator(),
                    new MinAggregateDefinition().CreateAggregatable(typeof(int)).CreateAggregator()
                };

                var function = new IteratorFunction(() => input.RowBuffer[1]);

                var argumentFunctions = new[] { function, function };

                using (var iterator = new StreamAggregateIterator(input, groupEntries, comparers, aggregators, argumentFunctions))
                {
                    AssertProduces(iterator, expected);
                }
            }
        }
    }
}