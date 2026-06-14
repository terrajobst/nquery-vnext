using System.Collections;
using System.Collections.Immutable;

using NQuery.Iterators;
using NQuery.Symbols.Aggregation;

namespace NQuery.Tests.Iterators;

// The emitted iterator takes group *indices* into the read buffer and EmittedFunctions that receive it.
// With outer == null the read buffer is just the input, so a group index is a column
// index and an argument function reads rb[column].
public class EmittedStreamAggregateIteratorTests : IteratorTests
{
    [Fact]
    public void Iterators_EmittedStreamAggregate_ForwardsProperly()
    {
        var rows = new object[] { 1, 2 };
        var expected = new object[1, 0];

        using var input = new MockedIterator(rows);

        using (var iterator = new EmittedStreamAggregateIterator(input, ImmutableArray<int>.Empty,
                   ImmutableArray<IComparer>.Empty, ImmutableArray<IAggregator>.Empty,
                   ImmutableArray<EmittedFunction>.Empty, outer: null))
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
            ImmutableArray<IAggregator>.Empty, ImmutableArray<EmittedFunction>.Empty, outer: null);
        AssertEmpty(iterator);
    }

    [Fact]
    public void Iterators_EmittedStreamAggregate_ReturnsSingleRow_IfInputEmptyAndNotGrouped()
    {
        var rows = Array.Empty<object>();
        var expected = new object[1, 0];

        using var input = new MockedIterator(rows);

        using var iterator = new EmittedStreamAggregateIterator(input, ImmutableArray<int>.Empty,
            ImmutableArray<IComparer>.Empty, ImmutableArray<IAggregator>.Empty,
            ImmutableArray<EmittedFunction>.Empty, outer: null);
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
        var aggregators = ImmutableArray.Create(
            new MaxAggregateDefinition().CreateAggregatable(typeof(int))!.CreateAggregator(),
            new MinAggregateDefinition().CreateAggregatable(typeof(int))!.CreateAggregator());

        var function = new EmittedFunction(rb => rb[0]);
        var argumentFunctions = ImmutableArray.Create(function, function);

        using var iterator = new EmittedStreamAggregateIterator(input, ImmutableArray<int>.Empty,
            ImmutableArray<IComparer>.Empty, aggregators, argumentFunctions, outer: null);
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
        var aggregators = ImmutableArray.Create(
            new MaxAggregateDefinition().CreateAggregatable(typeof(int))!.CreateAggregator(),
            new MinAggregateDefinition().CreateAggregatable(typeof(int))!.CreateAggregator());

        var function = new EmittedFunction(rb => rb[1]);
        var argumentFunctions = ImmutableArray.Create(function, function);

        using var iterator = new EmittedStreamAggregateIterator(input, groupIndices, comparers,
            aggregators, argumentFunctions, outer: null);
        AssertProduces(iterator, expected);
    }

    // Hole: the outer-correlation path. The read buffer is (outer ++ input), so an
    // argument function can combine the outer row with each input row.
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
        var aggregators = ImmutableArray.Create(
            new MaxAggregateDefinition().CreateAggregatable(typeof(int))!.CreateAggregator());

        // rb[0] is the outer value (100), rb[1] is the input value.
        var argumentFunctions = ImmutableArray.Create<EmittedFunction>(rb => (int)rb[0] + (int)rb[1]);

        using var iterator = new EmittedStreamAggregateIterator(input, ImmutableArray<int>.Empty,
            ImmutableArray<IComparer>.Empty, aggregators, argumentFunctions, outer);
        AssertProduces(iterator, expected);
    }
}
