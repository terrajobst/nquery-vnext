using System.Collections.Immutable;

using NQuery.Iterators;

namespace NQuery.Tests.Iterators
{
    public class ConcatenationIteratorTests : IteratorTests
    {
        [Fact]
        public void Iterators_Concatenation_ForwardsProperly()
        {
            var inputs = new[]
            {
                new MockedIterator(new object[] {1, 2}),
                new MockedIterator(new object[] {3, 4}),
                new MockedIterator(new object[] {5, 6})
            };

            var entries = new[]
            {
                ImmutableArray.Create(new RowBufferEntry(inputs[0].RowBuffer, 0)),
                ImmutableArray.Create(new RowBufferEntry(inputs[1].RowBuffer, 0)),
                ImmutableArray.Create(new RowBufferEntry(inputs[2].RowBuffer, 0))
            };

            var expected = new object[] { 1, 2, 3, 4, 5, 6 };

            using (var iterator = new ConcatenationIterator(inputs, entries))
            {
                for (var i = 0; i < 2; i++)
                {
                    AssertProduces(iterator, expected);
                }
            }

            foreach (var input in inputs)
            {
                Assert.Equal(2, input.TotalOpenCount);
                Assert.Equal(4, input.TotalReadCount);
                Assert.Equal(1, input.DisposalCount);
            }
        }

        [Fact]
        public void Iterators_Concatenation_ReturnsEmpty_IfAllEmpty()
        {
            var inputs = new[]
            {
                new MockedIterator(Array.Empty<object>()),
                new MockedIterator(Array.Empty<object>())
            };

            var entries = inputs.Select(_ => ImmutableArray<RowBufferEntry>.Empty);

            using (var iterator = new ConcatenationIterator(inputs, entries))
            {
                AssertEmpty(iterator);
            }
        }

        [Fact]
        public void Iterators_Concatenation_SkipsEmpty()
        {
            var inputs = new[]
            {
                new MockedIterator(Array.Empty<object>()),
                new MockedIterator(new object[] {5, 6})
            };

            var expected = new object[] { 5, 6 };

            var entries = inputs.Select(i => ImmutableArray.Create(new RowBufferEntry(i.RowBuffer, 0)));

            using (var iterator = new ConcatenationIterator(inputs, entries))
            {
                AssertProduces(iterator, expected);
            }
        }
    }
}