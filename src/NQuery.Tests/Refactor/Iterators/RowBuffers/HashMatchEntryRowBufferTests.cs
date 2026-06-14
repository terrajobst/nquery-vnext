using NQuery.Iterators;

namespace NQuery.Tests.Refactor.Iterators.RowBuffers;

public class HashMatchEntryRowBufferTests : RowBufferTests
{
    [Fact]
    public void RowBuffers_HashMatchEntry_ExposesEntryRowValues()
    {
        var buffer = new HashMatchEntryRowBuffer
        {
            Entry = new HashMatchEntry { RowValues = new object[] { 1, "Two", null! } }
        };

        AssertContract(buffer, 1, "Two", null);
    }

    [Fact]
    public void RowBuffers_HashMatchEntry_ReflectsEntrySwap()
    {
        var buffer = new HashMatchEntryRowBuffer
        {
            Entry = new HashMatchEntry { RowValues = new object[] { 1 } }
        };
        AssertContract(buffer, 1);

        buffer.Entry = new HashMatchEntry { RowValues = new object[] { 2, 3 } };
        AssertContract(buffer, 2, 3);
    }
}
