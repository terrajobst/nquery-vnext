using System.Collections.Immutable;

using NQuery.CodeAnalysis.Iterators;

namespace NQuery.Tests.CodeAnalysis.Iterators;

public class EmittedConcatenationIteratorTests : IteratorTests
{
    [Fact]
    public void Iterators_EmittedConcatenation_ForwardsProperly()
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

        using (var iterator = new EmittedConcatenationIterator(inputs, entries))
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
    public void Iterators_EmittedConcatenation_ReturnsEmpty_IfAllEmpty()
    {
        var inputs = new[]
        {
            new MockedIterator(Array.Empty<object>()),
            new MockedIterator(Array.Empty<object>())
        };

        var entries = inputs.Select(_ => ImmutableArray<RowBufferEntry>.Empty);

        using var iterator = new EmittedConcatenationIterator(inputs, entries);
        AssertEmpty(iterator);
    }

    [Fact]
    public void Iterators_EmittedConcatenation_SkipsEmpty()
    {
        var inputs = new[]
        {
            new MockedIterator(Array.Empty<object>()),
            new MockedIterator(new object[] {5, 6})
        };

        var expected = new object[] { 5, 6 };

        var entries = inputs.Select(i => ImmutableArray.Create(new RowBufferEntry(i.RowBuffer, 0)));

        using var iterator = new EmittedConcatenationIterator(inputs, entries);
        AssertProduces(iterator, expected);
    }

    // Hole: each input maps its own columns into the unified output order. Here the
    // second input's columns are reversed, and its entries reorder them to match the
    // first input's (value, name) layout.
    [Fact]
    public void Iterators_EmittedConcatenation_RemapsColumnsPerInput()
    {
        using var first = new MockedIterator(new object?[,]
        {
            {1, "One"}
        });
        using var second = new MockedIterator(new object?[,]
        {
            {"Two", 2}
        });

        var inputs = new[] { first, second };
        var entries = new[]
        {
            // first is already (value, name)
            ImmutableArray.Create(new RowBufferEntry(first.RowBuffer, 0), new RowBufferEntry(first.RowBuffer, 1)),
            // second is (name, value) -> reorder to (value, name)
            ImmutableArray.Create(new RowBufferEntry(second.RowBuffer, 1), new RowBufferEntry(second.RowBuffer, 0))
        };

        var expected = new object?[,]
        {
            {1, "One"},
            {2, "Two"}
        };

        using var iterator = new EmittedConcatenationIterator(inputs, entries);
        AssertProduces(iterator, expected);
    }
}
