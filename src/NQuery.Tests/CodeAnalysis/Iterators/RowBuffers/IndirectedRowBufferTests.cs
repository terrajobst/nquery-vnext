using NQuery.CodeAnalysis.Iterators;

namespace NQuery.Tests.CodeAnalysis.Iterators.RowBuffers;

public class IndirectedRowBufferTests : RowBufferTests
{
    [Fact]
    public void RowBuffers_Indirected_ExposesActiveBuffer()
    {
        var buffer = new IndirectedRowBuffer(Buffer(1, 2), Buffer(1, 2));
        AssertContract(buffer, 1, 2);
    }

    [Fact]
    public void RowBuffers_Indirected_SwapsActiveBuffer()
    {
        var first = Buffer(1, 2);
        var second = Buffer(3, 4);
        var buffer = new IndirectedRowBuffer(first, first);

        AssertContract(buffer, 1, 2);

        buffer.ActiveRowBuffer = second;
        AssertContract(buffer, 3, 4);
    }

    [Fact]
    public void RowBuffers_Indirected_CountIsFixedAtConstruction()
    {
        var buffer = new IndirectedRowBuffer(Buffer(1, 2), Buffer(1, 2));
        Assert.Equal(2, buffer.Bits32Count);

        buffer.ActiveRowBuffer = Buffer(9, 9);
        Assert.Equal(2, buffer.Bits32Count);
    }
}
