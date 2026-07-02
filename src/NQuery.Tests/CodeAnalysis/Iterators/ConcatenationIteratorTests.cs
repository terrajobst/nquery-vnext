using System.Collections.Immutable;

using NQuery.CodeAnalysis.Iterators;

namespace NQuery.Tests.CodeAnalysis.Iterators;

public class ConcatenationIteratorTests : IteratorTests
{
    private static readonly Type[] IntColumn = [typeof(int)];

    [Fact]
    public void Iterators_Concatenation_ForwardsProperly()
    {
        var inputs = new[]
        {
            new MockedIterator([1, 2]),
            new MockedIterator([3, 4]),
            new MockedIterator([5, 6])
        };

        var entries = new[]
        {
            ImmutableArray.Create(RowBufferTestSupport.Entry(inputs[0].RowBuffer, IntColumn, 0)),
            ImmutableArray.Create(RowBufferTestSupport.Entry(inputs[1].RowBuffer, IntColumn, 0)),
            ImmutableArray.Create(RowBufferTestSupport.Entry(inputs[2].RowBuffer, IntColumn, 0))
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
            new MockedIterator([]),
            new MockedIterator([])
        };

        var entries = inputs.Select(_ => ImmutableArray<RowBufferEntry>.Empty);

        using var iterator = new ConcatenationIterator(inputs, entries);
        AssertEmpty(iterator);
    }

    [Fact]
    public void Iterators_Concatenation_SkipsEmpty()
    {
        var inputs = new[]
        {
            new MockedIterator([]),
            new MockedIterator([5, 6])
        };

        var expected = new object[] { 5, 6 };

        var entries = inputs.Select(i => ImmutableArray.Create(RowBufferTestSupport.Entry(i.RowBuffer, IntColumn, 0)));

        using var iterator = new ConcatenationIterator(inputs, entries);
        AssertProduces(iterator, expected);
    }

    // Hole: each input maps its own columns into the unified output order. Here the
    // second input's columns are reversed, and its entries reorder them to match the
    // first input's (value, name) layout.
    [Fact]
    public void Iterators_Concatenation_RemapsColumnsPerInput()
    {
        using var first = new MockedIterator(new object?[,]
        {
            {1, "One"}
        });
        using var second = new MockedIterator(new object?[,]
        {
            {"Two", 2}
        });

        var firstTypes = new[] { typeof(int), typeof(string) };
        var secondTypes = new[] { typeof(string), typeof(int) };

        var inputs = new[] { first, second };
        var entries = new[]
        {
            // first is already (value, name)
            ImmutableArray.Create(RowBufferTestSupport.Entry(first.RowBuffer, firstTypes, 0), RowBufferTestSupport.Entry(first.RowBuffer, firstTypes, 1)),
            // second is (name, value) -> reorder to (value, name)
            ImmutableArray.Create(RowBufferTestSupport.Entry(second.RowBuffer, secondTypes, 1), RowBufferTestSupport.Entry(second.RowBuffer, secondTypes, 0))
        };

        var expected = new object?[,]
        {
            {1, "One"},
            {2, "Two"}
        };

        using var iterator = new ConcatenationIterator(inputs, entries);
        AssertProduces(iterator, expected);
    }
}
