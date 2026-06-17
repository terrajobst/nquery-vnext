using NQuery.CodeAnalysis.Iterators;

namespace NQuery.Tests.CodeAnalysis.Iterators.RowBuffers;

public class NullRowBufferTests : RowBufferTests
{
    [Fact]
    public void RowBuffers_Null_Empty()
    {
        var buffer = new NullRowBuffer(0, 0, 0, 0);
        AssertContract(buffer);
    }

    [Fact]
    public void RowBuffers_Null_IsAllNull()
    {
        var buffer = new NullRowBuffer(3, 0, 0, 0);
        AssertContract(buffer, null, null, null);
    }

    [Fact]
    public void RowBuffers_Null_IsAllNull_AcrossContainers()
    {
        // Two object columns, one 32-bit column, one 64-bit column -- all NULL.
        var buffer = new NullRowBuffer(2, 1, 1, 0);

        Assert.Null(buffer.GetObject(0));
        Assert.Null(buffer.GetObject(1));
        Assert.True(buffer.IsNull32(0));
        Assert.True(buffer.IsNull64(0));
    }
}
