using System.Collections;
using System.Collections.Immutable;

using NQuery.CodeAnalysis.Iterators;

namespace NQuery.Tests.CodeAnalysis.Iterators;

// The emitted iterator drives the grouping/run logic and calls three delegates (initialize,
// accumulate, store) that the emitter compiles. The tests here stand in hand-written delegates
// for those, so they exercise the iterator rather than the real aggregate compilation.
public class EmittedStreamAggregateIteratorTests : IteratorTests
{
    private static EmittedAggregates None()
    {
        return new EmittedAggregates(() => { }, _ => { }, _ => { });
    }

    // MAX and MIN over the int in the given column, written at outputOffset / outputOffset + 1.
    private static EmittedAggregates MaxMin(int column, int outputOffset)
    {
        int? max = null;
        int? min = null;
        return new EmittedAggregates(
            () => { max = null; min = null; },
            rb =>
            {
                var v = (int)rb[column]!;
                max = max is null ? v : Math.Max(max.Value, v);
                min = min is null ? v : Math.Min(min.Value, v);
            },
            target =>
            {
                target[outputOffset] = max;
                target[outputOffset + 1] = min;
            });
    }

    [Fact]
    public void Iterators_EmittedStreamAggregate_ForwardsProperly()
    {
        var rows = new object[] { 1, 2 };
        var expected = new object[1, 0];

        using var input = new MockedIterator(rows);

        using (var iterator = new EmittedStreamAggregateIterator(input, ImmutableArray<int>.Empty,
                   ImmutableArray<IComparer>.Empty, None(), aggregateCount: 0, outer: null))
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
    public void Iterators_EmittedStreamAggregate_ReturnsEmpty_IfInputEmptyAndGrouped()
    {
        var rows = Array.Empty<object>();

        using var input = new MockedIterator(rows);
        var groupIndices = ImmutableArray.Create(0);
        var comparers = ImmutableArray.Create<IComparer>(Comparer.Default);

        using var iterator = new EmittedStreamAggregateIterator(input, groupIndices, comparers,
            None(), aggregateCount: 0, outer: null);
        AssertEmpty(iterator);
    }

    [Fact]
    public void Iterators_EmittedStreamAggregate_ReturnsSingleRow_IfInputEmptyAndNotGrouped()
    {
        var rows = Array.Empty<object>();
        var expected = new object[1, 0];

        using var input = new MockedIterator(rows);

        using var iterator = new EmittedStreamAggregateIterator(input, ImmutableArray<int>.Empty,
            ImmutableArray<IComparer>.Empty, None(), aggregateCount: 0, outer: null);
        AssertProduces(iterator, expected);
    }

    [Fact]
    public void Iterators_EmittedStreamAggregate_ComputeAggregates_WhenNotGrouped()
    {
        var rows = new object[] { 1, 2, 3 };
        var expected = new object?[,]
        {
            {3, 1}
        };

        using var input = new MockedIterator(rows);

        using var iterator = new EmittedStreamAggregateIterator(input, ImmutableArray<int>.Empty,
            ImmutableArray<IComparer>.Empty, MaxMin(column: 0, outputOffset: 0), aggregateCount: 2, outer: null);
        AssertProduces(iterator, expected);
    }

    [Fact]
    public void Iterators_EmittedStreamAggregate_ComputeAggregates_WhenGrouped()
    {
        var rows = new object?[,]
        {
            {"One", 1},
            {"One", 2},
            {"Two", 3}
        };

        var expected = new object?[,]
        {
            {"One", 2, 1},
            {"Two", 3, 3}
        };

        using var input = new MockedIterator(rows);
        var groupIndices = ImmutableArray.Create(0);
        var comparers = ImmutableArray.Create<IComparer>(Comparer.Default);

        using var iterator = new EmittedStreamAggregateIterator(input, groupIndices, comparers,
            MaxMin(column: 1, outputOffset: 1), aggregateCount: 2, outer: null);
        AssertProduces(iterator, expected);
    }

    // Hole: the outer-correlation path. The read buffer is (outer ++ input), so an
    // argument can combine the outer row with each input row.
    [Fact]
    public void Iterators_EmittedStreamAggregate_ComputeAggregates_UsingOuter()
    {
        var rows = new object[] { 1, 2, 3 };
        var expected = new object?[,]
        {
            {103}
        };
        var outer = new MockedRowBuffer(new object[] { 100 });

        using var input = new MockedIterator(rows);

        // rb[0] is the outer value (100), rb[1] is the input value; MAX over their sum.
        int? max = null;
        var aggregates = new EmittedAggregates(
            () => max = null,
            rb =>
            {
                var v = (int)rb[0]! + (int)rb[1]!;
                max = max is null ? v : Math.Max(max.Value, v);
            },
            target => target[0] = max);

        using var iterator = new EmittedStreamAggregateIterator(input, ImmutableArray<int>.Empty,
            ImmutableArray<IComparer>.Empty, aggregates, aggregateCount: 1, outer);
        AssertProduces(iterator, expected);
    }
}
