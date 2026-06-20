using NQuery.CodeAnalysis.Iterators;

namespace NQuery.Tests.CodeAnalysis.Iterators.RowBuffers;

public class HashMatchRowBufferTests : RowBufferTests
{
    private static readonly Type[] Int2Int = { typeof(int), typeof(int), typeof(int) };

    [Fact]
    public void RowBuffers_HashMatch_ExposesBuildEntryThenProbe()
    {
        var buffer = new HashMatchRowBuffer(Buffer(0, 0), Buffer(0));
        buffer.SetBuild(Buffer(1, 2));
        buffer.SetProbe(Buffer(9));

        AssertContract(buffer, 1, 2, 9);
    }

    [Fact]
    public void RowBuffers_HashMatch_NullBuild_PadsBuildSideWithNull()
    {
        var buffer = new HashMatchRowBuffer(Buffer(0, 0), Buffer(0));
        buffer.SetBuild(null);
        buffer.SetProbe(Buffer(9));

        AssertContract(buffer, new object?[] { null, null, 9 }, Int2Int);
    }

    [Fact]
    public void RowBuffers_HashMatch_NullProbe_PadsProbeSideWithNull()
    {
        var buffer = new HashMatchRowBuffer(Buffer(0, 0), Buffer(0));
        buffer.SetBuild(Buffer(1, 2));
        buffer.SetProbe(null);

        AssertContract(buffer, new object?[] { 1, 2, null }, Int2Int);
    }

    [Fact]
    public void RowBuffers_HashMatch_ReflectsBuildAndProbeSwaps()
    {
        var buffer = new HashMatchRowBuffer(Buffer(0), Buffer("x"));

        buffer.SetBuild(Buffer(1));
        buffer.SetProbe(Buffer("a"));
        AssertContract(buffer, 1, "a");

        buffer.SetBuild(Buffer(2));
        buffer.SetProbe(Buffer("b"));
        AssertContract(buffer, 2, "b");
    }
}
