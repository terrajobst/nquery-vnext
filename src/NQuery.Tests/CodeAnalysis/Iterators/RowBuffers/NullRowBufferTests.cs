using NQuery.CodeAnalysis.Iterators;

namespace NQuery.Tests.CodeAnalysis.Iterators.RowBuffers;

public class NullRowBufferTests : RowBufferTests
{
    [Fact]
    public void RowBuffers_Null_Empty()
    {
        var buffer = new NullRowBuffer(0);
        AssertContract(buffer);
    }

    [Fact]
    public void RowBuffers_Null_IsAllNull()
    {
        var buffer = new NullRowBuffer(3);
        AssertContract(buffer, null, null, null);
    }

    [Fact]
    public void RowBuffers_Null_CopyTo_OverwritesWithNull()
    {
        var buffer = new NullRowBuffer(2);
        var array = new object[] { "a", "b", "c" };

        buffer.CopyTo(array, 1);

        Assert.Equal(new object?[] { "a", null, null }, array);
    }
}
