using NQuery.CodeAnalysis.Iterators;

namespace NQuery.Tests.CodeAnalysis.Iterators.RowBuffers;

public class EmptyRowBufferTests : RowBufferTests
{
    [Fact]
    public void RowBuffers_Empty_HasNoColumns()
    {
        var buffer = new EmptyRowBuffer();
        AssertContract(buffer);
    }

    [Fact]
    public void RowBuffers_Empty_AccessThrows()
    {
        var buffer = new EmptyRowBuffer();
        Assert.Throws<ArgumentOutOfRangeException>(() => buffer.GetObject(0));
    }
}
