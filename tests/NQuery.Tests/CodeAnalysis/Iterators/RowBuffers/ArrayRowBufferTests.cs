using NQuery.CodeAnalysis.Iterators;

namespace NQuery.Tests.CodeAnalysis.Iterators.RowBuffers;

public class ArrayRowBufferTests : RowBufferTests
{
    [Fact]
    public void RowBuffers_Array_Empty()
    {
        var buffer = new ArrayRowBuffer(0, 0, 0, 0);
        AssertContract(buffer);
    }

    [Fact]
    public void RowBuffers_Array_ExposesValues()
    {
        // One 32-bit column, one object column, one (null) object column.
        var buffer = new ArrayRowBuffer(2, 1, 0, 0);
        buffer.Write32Bit<int>(0, 1);
        buffer.WriteObject<string?>(0, "Two");
        buffer.WriteObject<string?>(1, null);

        AssertContract(buffer, [1, "Two", null], [typeof(int), typeof(string), typeof(string)]);
    }

    [Fact]
    public void RowBuffers_Array_ReflectsBackingWrites()
    {
        var buffer = new ArrayRowBuffer(0, 1, 0, 0);
        buffer.Write32Bit<int>(0, 1);
        Assert.Equal(1, buffer.Read32Bit<int>(0));

        buffer.Write32Bit<int>(0, 2);
        Assert.Equal(2, buffer.Read32Bit<int>(0));
    }

    [Fact]
    public void RowBuffers_Array_TracksNullPerColumn()
    {
        var buffer = new ArrayRowBuffer(0, 2, 0, 0);
        buffer.Write32Bit<int>(0, 5);
        buffer.Write32Bit<int>(1, null);

        Assert.Equal(5, buffer.Read32Bit<int>(0));
        Assert.Null(buffer.Read32Bit<int>(1));
        Assert.False(buffer.IsNull32(0));
        Assert.True(buffer.IsNull32(1));
    }

    [Fact]
    public void RowBuffers_Array_RoundTripsEachWidth()
    {
        var buffer = new ArrayRowBuffer(0, 1, 1, 1);
        buffer.Write32Bit<float>(0, 1.5f);
        buffer.Write64Bit<double>(0, 2.5);
        buffer.Write128Bit<decimal>(0, 3.5m);

        Assert.Equal(1.5f, buffer.Read32Bit<float>(0));
        Assert.Equal(2.5, buffer.Read64Bit<double>(0));
        Assert.Equal(3.5m, buffer.Read128Bit<decimal>(0));
    }
}
