using System.Collections.Immutable;

using NQuery.Refactor.Iterators;

namespace NQuery.Tests.Refactor.Iterators
{
    public class EmittedComputeScalarIteratorTests : IteratorTests
    {
        [Fact]
        public void Iterators_EmittedComputeScalar_ForwardsProperly()
        {
            var values = ImmutableArray<EmittedFunction>.Empty;

            var rows = new object[] { 1, 2 };
            var expected = rows;

            using var input = new MockedIterator(rows);
            using (var iterator = new EmittedComputeScalarIterator(input, values, outer: null))
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

        [Fact]
        public void Iterators_EmittedComputeScalar_ReturnsEmpty_IfInputIsEmpty()
        {
            var rows = Array.Empty<object>();
            var values = ImmutableArray.Create<EmittedFunction>(_ => 1);

            using var input = new MockedIterator(rows);
            using var iterator = new EmittedComputeScalarIterator(input, values, outer: null);

            AssertEmpty(iterator);
        }

        [Fact]
        public void Iterators_EmittedComputeScalar_ComputesValues()
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

            using var input = new MockedIterator(rows);
            var values = ImmutableArray.Create<EmittedFunction>(rb => (int)rb[0] * 3);
            using var iterator = new EmittedComputeScalarIterator(input, values, outer: null);

            AssertProduces(iterator, expected);
        }

        // Hole: the outer-correlation path. The function sees (outer ++ input); the computed
        // column is still appended to the input's own columns.
        [Fact]
        public void Iterators_EmittedComputeScalar_ComputesValues_UsingOuter()
        {
            var rows = new object[] { 4, 6, 8 };
            var expected = new object[,]
            {
                {4, 14},
                {6, 16},
                {8, 18}
            };
            var outer = new MockedRowBuffer(new object[] { 10 });

            using var input = new MockedIterator(rows);
            // rb[0] is the outer value (10), rb[1] is the input value -> outer + input.
            var values = ImmutableArray.Create<EmittedFunction>(rb => (int)rb[0] + (int)rb[1]);
            using var iterator = new EmittedComputeScalarIterator(input, values, outer);

            AssertProduces(iterator, expected);
        }
    }
}
