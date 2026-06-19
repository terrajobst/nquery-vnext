using NQuery.CodeAnalysis.Iterators;

namespace NQuery.Tests.CodeAnalysis.Iterators;

public class EmittedComputeScalarIteratorTests : IteratorTests
{
    [Fact]
    public void Iterators_EmittedComputeScalar_ForwardsProperly()
    {
        Action<RowBuffer, ArrayRowBuffer> writer = (_, _) => { };
        var layout = RowBufferLayout.Create(Array.Empty<Type>());

        var rows = new object[] { 1, 2 };
        var expected = rows;

        using var input = new MockedIterator(rows);
        using (var iterator = new EmittedComputeScalarIterator(input, writer, layout, outer: null))
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
        var layout = RowBufferLayout.Create(new[] { typeof(int) });
        Action<RowBuffer, ArrayRowBuffer> writer = (_, target) => target.Write32Bit<int>(0, 1);

        using var input = new MockedIterator(rows);
        using var iterator = new EmittedComputeScalarIterator(input, writer, layout, outer: null);

        AssertEmpty(iterator);
    }

    [Fact]
    public void Iterators_EmittedComputeScalar_ComputesValues()
    {
        var rows = new object[]
        {
            4, 6, 8
        };

        var expected = new object?[,]
        {
            {4, 12},
            {6, 18},
            {8, 24}
        };

        using var input = new MockedIterator(rows);
        var layout = RowBufferLayout.Create(new[] { typeof(int) });
        Action<RowBuffer, ArrayRowBuffer> writer = (rb, target) => target.Write32Bit<int>(0, rb.NullableInt32(0) * 3);
        using var iterator = new EmittedComputeScalarIterator(input, writer, layout, outer: null);

        AssertProduces(iterator, expected);
    }

    // Hole: the outer-correlation path. The function sees (outer ++ input); the computed
    // column is still appended to the input's own columns.
    [Fact]
    public void Iterators_EmittedComputeScalar_ComputesValues_UsingOuter()
    {
        var rows = new object[] { 4, 6, 8 };
        var expected = new object?[,]
        {
            {4, 14},
            {6, 16},
            {8, 18}
        };
        var outer = new MockedRowBuffer(new object[] { 10 });

        using var input = new MockedIterator(rows);
        var layout = RowBufferLayout.Create(new[] { typeof(int) });
        // rb.NullableInt32(0) is the outer value (10), rb.NullableInt32(1) is the input value -> outer + input.
        Action<RowBuffer, ArrayRowBuffer> writer = (rb, target) => target.Write32Bit<int>(0, rb.NullableInt32(0) + rb.NullableInt32(1));
        using var iterator = new EmittedComputeScalarIterator(input, writer, layout, outer);

        AssertProduces(iterator, expected);
    }
}
