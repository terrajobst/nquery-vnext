using NQuery.Iterators;

namespace NQuery.Tests.Iterators.RowBuffers;

public class ArrayRowBufferTests : RowBufferTests
{
    [Fact]
    public void RowBuffers_Array_Empty()
    {
        var buffer = new ArrayRowBuffer(0);
        AssertContract(buffer);
    }

    [Fact]
    public void RowBuffers_Array_ExposesValues()
    {
        var buffer = new ArrayRowBuffer(3);
        buffer.Array[0] = 1;
        buffer.Array[1] = "Two";
        buffer.Array[2] = null!;

        AssertContract(buffer, 1, "Two", null);
    }

    [Fact]
    public void RowBuffers_Array_ReflectsBackingWrites()
    {
        var buffer = new ArrayRowBuffer(1);
        buffer.Array[0] = 1;
        Assert.Equal(1, buffer[0]);

        buffer.Array[0] = 2;
        Assert.Equal(2, buffer[0]);
    }
}
