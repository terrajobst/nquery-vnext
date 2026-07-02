using NQuery.CodeAnalysis.Iterators;

namespace NQuery.Tests.CodeAnalysis.Iterators.RowBuffers;

public class ProjectedRowBufferTests : RowBufferTests
{
    [Fact]
    public void RowBuffers_Projected_Empty()
    {
        var buffer = new ProjectedRowBuffer([]);
        AssertContract(buffer);
    }

    [Fact]
    public void RowBuffers_Projected_ReordersColumns()
    {
        var types = new[] { typeof(int), typeof(string) };
        var source = Buffer(1, "Two");
        var entries = new[]
        {
            RowBufferTestSupport.Entry(source, types, 1),
            RowBufferTestSupport.Entry(source, types, 0)
        };

        var buffer = new ProjectedRowBuffer(entries);
        AssertContract(buffer, "Two", 1);
    }

    [Fact]
    public void RowBuffers_Projected_SelectsSubset()
    {
        var types = new[] { typeof(int), typeof(int), typeof(int) };
        var source = Buffer(1, 2, 3);
        var entries = new[] { RowBufferTestSupport.Entry(source, types, 1) };

        var buffer = new ProjectedRowBuffer(entries);
        AssertContract(buffer, 2);
    }

    [Fact]
    public void RowBuffers_Projected_ProjectsAcrossMultipleSources()
    {
        var leftTypes = new[] { typeof(string), typeof(string) };
        var rightTypes = new[] { typeof(string) };
        var left = Buffer("L0", "L1");
        var right = Buffer("R0");
        var entries = new[]
        {
            RowBufferTestSupport.Entry(right, rightTypes, 0),
            RowBufferTestSupport.Entry(left, leftTypes, 1)
        };

        var buffer = new ProjectedRowBuffer(entries);
        AssertContract(buffer, "R0", "L1");
    }

    [Fact]
    public void RowBuffers_Projected_ReflectsUnderlyingWrites()
    {
        var types = new[] { typeof(int) };
        var source = Buffer(1);
        var buffer = new ProjectedRowBuffer(new[] { RowBufferTestSupport.Entry(source, types, 0) });

        source.Write32Bit<int>(0, 42);
        AssertContract(buffer, 42);
    }
}
