using System.Collections;
using System.Collections.Immutable;

using NQuery.CodeAnalysis.Iterators;

namespace NQuery.Tests.CodeAnalysis.Iterators;

// The emitted iterator drives the grouping/run logic and calls three delegates (initialize,
// accumulate, store) that the emitter compiles. The tests here stand in hand-written delegates
// for those, so they exercise the iterator rather than the real aggregate compilation. The
// output buffer lays out the grouping columns first, then the aggregate results.
public class EmittedStreamAggregateIteratorTests : IteratorTests
{
    private static EmittedAggregates None()
    {
        return new EmittedAggregates(() => { }, _ => { }, _ => { });
    }

    // Builds the iterator the way the executable operator does: grouping columns are read
    // from the input layout and written to the leading output columns; the aggregate
    // results occupy the trailing output columns.
    private static EmittedStreamAggregateIterator CreateAggregate(MockedIterator input, Type[] inputTypes, int[] groupColumns, Type[] aggregateTypes, EmittedAggregates aggregates, RowBuffer? outer = null)
    {
        var inputLayout = RowBufferLayout.Create(inputTypes);
        var groupTypes = groupColumns.Select(i => inputTypes[i]).ToArray();
        var groupSource = groupColumns.Select(i => inputLayout.Columns[i]).ToImmutableArray();

        var outputTypes = groupTypes.Concat(aggregateTypes).ToArray();
        var outputLayout = RowBufferLayout.Create(outputTypes);
        var groupOutput = outputLayout.Columns.Take(groupColumns.Length).ToImmutableArray();
        var comparers = groupColumns.Select(_ => (IComparer)Comparer.Default).ToImmutableArray();

        return new EmittedStreamAggregateIterator(input, groupSource, groupOutput, groupTypes.ToImmutableArray(), comparers, aggregates, outputLayout, outer);
    }

    // MAX and MIN over the int in the given 32-bit column, written to the first two
    // aggregate (32-bit) output columns.
    private static EmittedAggregates MaxMin(int valueColumn)
    {
        int? max = null;
        int? min = null;
        return new EmittedAggregates(
            () => { max = null; min = null; },
            rb =>
            {
                var v = rb.NullableInt32(valueColumn)!.Value;
                max = max is null ? v : Math.Max(max.Value, v);
                min = min is null ? v : Math.Min(min.Value, v);
            },
            target =>
            {
                target.Write32Bit<int>(0, max);
                target.Write32Bit<int>(1, min);
            });
    }

    [Fact]
    public void Iterators_EmittedStreamAggregate_ForwardsProperly()
    {
        var rows = new object[] { 1, 2 };
        var expected = new object[1, 0];

        using var input = new MockedIterator(rows);

        using (var iterator = CreateAggregate(input, new[] { typeof(int) }, Array.Empty<int>(), Array.Empty<Type>(), None()))
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

        using var iterator = CreateAggregate(input, new[] { typeof(object) }, new[] { 0 }, Array.Empty<Type>(), None());
        AssertEmpty(iterator);
    }

    [Fact]
    public void Iterators_EmittedStreamAggregate_ReturnsSingleRow_IfInputEmptyAndNotGrouped()
    {
        var rows = Array.Empty<object>();
        var expected = new object[1, 0];

        using var input = new MockedIterator(rows);

        using var iterator = CreateAggregate(input, new[] { typeof(object) }, Array.Empty<int>(), Array.Empty<Type>(), None());
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

        using var iterator = CreateAggregate(input, new[] { typeof(int) }, Array.Empty<int>(), new[] { typeof(int), typeof(int) }, MaxMin(valueColumn: 0));
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

        // Group by the (object) name column; the value int is the input's only 32-bit column.
        using var iterator = CreateAggregate(input, new[] { typeof(string), typeof(int) }, new[] { 0 }, new[] { typeof(int), typeof(int) }, MaxMin(valueColumn: 0));
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

        // rb.NullableInt32(0) is the outer value (100), rb.NullableInt32(1) is the input value; MAX over their sum.
        int? max = null;
        var aggregates = new EmittedAggregates(
            () => max = null,
            rb =>
            {
                var v = rb.NullableInt32(0)!.Value + rb.NullableInt32(1)!.Value;
                max = max is null ? v : Math.Max(max.Value, v);
            },
            target => target.Write32Bit<int>(0, max));

        using var iterator = CreateAggregate(input, new[] { typeof(int) }, Array.Empty<int>(), new[] { typeof(int) }, aggregates, outer);
        AssertProduces(iterator, expected);
    }
}
