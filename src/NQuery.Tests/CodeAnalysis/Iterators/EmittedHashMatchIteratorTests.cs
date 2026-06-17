using NQuery.CodeAnalysis.Iterators;

namespace NQuery.Tests.CodeAnalysis.Iterators;

// The hash-match iterator uses two booleans to configure its join kind:
//   Inner       = (preserveBuild: false, preserveProbe: false)
//   LeftOuter   = (preserveBuild: true,  preserveProbe: false)
//   RightOuter  = (preserveBuild: false, preserveProbe: true)
//   FullOuter   = (preserveBuild: true,  preserveProbe: true)
// The remainder predicate receives the combined (build ++ probe) buffer. Columns keep
// their container, so a flat output column maps to a container index per its type (e.g.
// for an (int, string) build joined to an (int, string) probe, the ints are 32-bit
// columns 0/1 and the strings are object columns 0/1).
public class EmittedHashMatchIteratorTests : IteratorTests
{
    private static EmittedHashMatchIterator Join(MockedIterator build, MockedIterator probe, EmittedPredicate remainder, bool preserveBuild, bool preserveProbe, bool semi = false, bool anti = false, bool probing = false)
    {
        var buildKey = RowBufferTestSupport.IntKey(build.RowBuffer, 0);
        var probeKey = RowBufferTestSupport.IntKey(probe.RowBuffer, 0);
        return new EmittedHashMatchIterator(build, probe, buildKey, probeKey, remainder, preserveBuild, preserveProbe, semi, anti, probing);
    }

    [Theory]
    [InlineData(false, false)] // Inner
    [InlineData(true, false)]  // LeftOuter
    [InlineData(false, true)]  // RightOuter
    [InlineData(true, true)]   // FullOuter
    public void Iterators_EmittedHashMatch_ForwardsProperly(bool preserveBuild, bool preserveProbe)
    {
        var buildRows = new object?[] { 1, 2 };
        var probeRows = new object?[] { 2, 3 };

        using var build = new MockedIterator(buildRows);
        using var probe = new MockedIterator(probeRows);

        const int passCount = 2;

        using (var iterator = Join(build, probe, _ => true, preserveBuild, preserveProbe))
        {
            for (var i = 0; i < passCount; i++)
            {
                iterator.Open();

                Assert.True(iterator.Read());
                Assert.Equal(2, iterator.RowBuffer.NullableInt32(0));
                Assert.Equal(2, iterator.RowBuffer.NullableInt32(1));

                if (preserveProbe)
                {
                    Assert.True(iterator.Read());
                    Assert.Null(iterator.RowBuffer.NullableInt32(0));
                    Assert.Equal(3, iterator.RowBuffer.NullableInt32(1));
                }

                if (preserveBuild)
                {
                    Assert.True(iterator.Read());
                    Assert.Equal(1, iterator.RowBuffer.NullableInt32(0));
                    Assert.Null(iterator.RowBuffer.NullableInt32(1));
                }

                Assert.False(iterator.Read());
            }
        }

        var inputs = new[] { build, probe };

        foreach (var input in inputs)
        {
            Assert.Equal(passCount, input.TotalOpenCount);
            Assert.Equal(passCount * 2, input.TotalReadCount);
            Assert.Equal(1, input.DisposalCount);
        }
    }

    [Theory]
    [InlineData(false, false)] // Inner
    [InlineData(true, false)]  // LeftOuter
    public void Iterators_EmittedHashMatch_ReturnsEmpty_IfBuildIsEmpty(bool preserveBuild, bool preserveProbe)
    {
        var probeRows = new object?[] { 2, 3 };

        using var build = MockedIterator.Empty(typeof(int));
        using var probe = new MockedIterator(probeRows);

        using var iterator = Join(build, probe, _ => true, preserveBuild, preserveProbe);
        AssertEmpty(iterator);
    }

    [Theory]
    [InlineData(false, true)] // RightOuter
    [InlineData(true, true)]  // FullOuter
    public void Iterators_EmittedHashMatch_ReturnsProbe_IfBuildIsEmpty(bool preserveBuild, bool preserveProbe)
    {
        var probeRows = new object?[] { 2, 3 };
        var expected = new object?[,]
        {
            {null, 2},
            {null, 3}
        };

        using var build = MockedIterator.Empty(typeof(int));
        using var probe = new MockedIterator(probeRows);

        using var iterator = Join(build, probe, _ => true, preserveBuild, preserveProbe);
        AssertProduces(iterator, new[] { typeof(int), typeof(int) }, expected);
    }

    [Theory]
    [InlineData(false, false)] // Inner
    [InlineData(false, true)]  // RightOuter
    public void Iterators_EmittedHashMatch_ReturnsEmpty_IfProbeIsEmpty(bool preserveBuild, bool preserveProbe)
    {
        var buildRows = new object?[] { 1, 2 };

        using var build = new MockedIterator(buildRows);
        using var probe = MockedIterator.Empty(typeof(int));

        using var iterator = Join(build, probe, _ => true, preserveBuild, preserveProbe);
        AssertEmpty(iterator);
    }

    [Theory]
    [InlineData(true, false)] // LeftOuter
    [InlineData(true, true)]  // FullOuter
    public void Iterators_EmittedHashMatch_ReturnsBuild_IfProbeIsEmpty(bool preserveBuild, bool preserveProbe)
    {
        var buildRows = new object?[] { 1, 2 };
        var expected = new object?[,]
        {
            {1, null},
            {2, null}
        };

        using var build = new MockedIterator(buildRows);
        using var probe = MockedIterator.Empty(typeof(int));

        using var iterator = Join(build, probe, _ => true, preserveBuild, preserveProbe);
        AssertProduces(iterator, new[] { typeof(int), typeof(int) }, expected);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Iterators_EmittedHashMatch_DoesNotMatchNulls(bool preserveBuild, bool preserveProbe)
    {
        var buildRows = new object?[,]
        {
            {null, "Foo"}
        };
        var probeRows = new object?[,]
        {
            {null, "Bar"}
        };

        var columnTypes = new[] { typeof(int), typeof(string) };
        using var build = new MockedIterator(columnTypes, buildRows);
        using var probe = new MockedIterator(columnTypes, probeRows);

        using var iterator = Join(build, probe, _ => true, preserveBuild, preserveProbe);
        iterator.Open();

        // 32-bit columns: build.int(0), probe.int(1); object columns: build.string(0), probe.string(1).
        if (preserveProbe)
        {
            Assert.True(iterator.Read());
            Assert.Null(iterator.RowBuffer.NullableInt32(0));
            Assert.Null(iterator.RowBuffer.String(0));
            Assert.Null(iterator.RowBuffer.NullableInt32(1));
            Assert.Equal("Bar", iterator.RowBuffer.String(1));
        }

        if (preserveBuild)
        {
            Assert.True(iterator.Read());
            Assert.Null(iterator.RowBuffer.NullableInt32(0));
            Assert.Equal("Foo", iterator.RowBuffer.String(0));
            Assert.Null(iterator.RowBuffer.NullableInt32(1));
            Assert.Null(iterator.RowBuffer.String(1));
        }

        Assert.False(iterator.Read());
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Iterators_EmittedHashMatch_MatchesDuplicates(bool preserveBuild, bool preserveProbe)
    {
        var buildRows = new object?[] { 1, 2, 3 };
        var probeRows = new object?[,]
        {
            {1, "First"},
            {2, "Second1"},
            {2, "Second2"},
            {3, "Third"}
        };
        var expected = new object?[,]
        {
            {1, 1, "First"},
            {2, 2, "Second1"},
            {2, 2, "Second2"},
            {3, 3, "Third"}
        };

        using var build = new MockedIterator(buildRows);
        using var probe = new MockedIterator(probeRows);

        using var iterator = Join(build, probe, _ => true, preserveBuild, preserveProbe);
        AssertProduces(iterator, expected);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Iterators_EmittedHashMatch_MatchesWithRemainder(bool preserveBuild, bool preserveProbe)
    {
        var buildRows = new object?[,]
        {
            {1, 1},
            {1, 2},
            {2, 1},
            {2, 2},
            {3, 1}
        };

        var probeRows = new object?[,]
        {
            {0, 0, "Unmatched1" },
            {1, 1, "Project1-Task-1"},
            {1, 2, "Project1-Task-2"},
            {1, 3, "Unmatched2"},
            {2, 1, "Project2-Task-1"},
        };

        using var build = new MockedIterator(buildRows);
        using var probe = new MockedIterator(probeRows);

        // 32-bit columns: build.ProjectId(0), build.TaskId(1), probe.ProjectId(2), probe.TaskId(3);
        // object columns: probe.TaskName(0). The remainder matches build.TaskId == probe.TaskId.
        var remainder = new EmittedPredicate(rb => Equals(rb.NullableInt32(1), rb.NullableInt32(3)));

        using var iterator = Join(build, probe, remainder, preserveBuild, preserveProbe);
        iterator.Open();

        if (preserveProbe)
        {
            Assert.True(iterator.Read());
            Assert.Null(iterator.RowBuffer.NullableInt32(0));
            Assert.Null(iterator.RowBuffer.NullableInt32(1));
            Assert.Equal(0, iterator.RowBuffer.NullableInt32(2));
            Assert.Equal(0, iterator.RowBuffer.NullableInt32(3));
            Assert.Equal("Unmatched1", iterator.RowBuffer.String(0));
        }

        Assert.True(iterator.Read());
        Assert.Equal(1, iterator.RowBuffer.NullableInt32(0));
        Assert.Equal(1, iterator.RowBuffer.NullableInt32(1));
        Assert.Equal(1, iterator.RowBuffer.NullableInt32(2));
        Assert.Equal(1, iterator.RowBuffer.NullableInt32(3));
        Assert.Equal("Project1-Task-1", iterator.RowBuffer.String(0));

        Assert.True(iterator.Read());
        Assert.Equal(1, iterator.RowBuffer.NullableInt32(0));
        Assert.Equal(2, iterator.RowBuffer.NullableInt32(1));
        Assert.Equal(1, iterator.RowBuffer.NullableInt32(2));
        Assert.Equal(2, iterator.RowBuffer.NullableInt32(3));
        Assert.Equal("Project1-Task-2", iterator.RowBuffer.String(0));

        if (preserveProbe)
        {
            Assert.True(iterator.Read());
            Assert.Null(iterator.RowBuffer.NullableInt32(0));
            Assert.Null(iterator.RowBuffer.NullableInt32(1));
            Assert.Equal(1, iterator.RowBuffer.NullableInt32(2));
            Assert.Equal(3, iterator.RowBuffer.NullableInt32(3));
            Assert.Equal("Unmatched2", iterator.RowBuffer.String(0));
        }

        Assert.True(iterator.Read());
        Assert.Equal(2, iterator.RowBuffer.NullableInt32(0));
        Assert.Equal(1, iterator.RowBuffer.NullableInt32(1));
        Assert.Equal(2, iterator.RowBuffer.NullableInt32(2));
        Assert.Equal(1, iterator.RowBuffer.NullableInt32(3));
        Assert.Equal("Project2-Task-1", iterator.RowBuffer.String(0));

        if (preserveBuild)
        {
            Assert.True(iterator.Read());
            Assert.Equal(2, iterator.RowBuffer.NullableInt32(0));
            Assert.Equal(2, iterator.RowBuffer.NullableInt32(1));
            Assert.Null(iterator.RowBuffer.NullableInt32(2));
            Assert.Null(iterator.RowBuffer.NullableInt32(3));
            Assert.Null(iterator.RowBuffer.String(0));

            Assert.True(iterator.Read());
            Assert.Equal(3, iterator.RowBuffer.NullableInt32(0));
            Assert.Equal(1, iterator.RowBuffer.NullableInt32(1));
            Assert.Null(iterator.RowBuffer.NullableInt32(2));
            Assert.Null(iterator.RowBuffer.NullableInt32(3));
            Assert.Null(iterator.RowBuffer.String(0));
        }

        Assert.False(iterator.Read());
    }

    // Semi/anti consume the probe only to mark matches and then output the build side:
    // semi keeps the build rows that matched, anti the ones that did not.
    [Fact]
    public void Iterators_EmittedHashMatch_Semi_EmitsMatchedBuildRows()
    {
        var buildRows = new object?[] { 1, 2, 3 };
        var probeRows = new object?[] { 2, 3, 4 };

        using var build = new MockedIterator(buildRows);
        using var probe = new MockedIterator(probeRows);

        using var iterator = Join(build, probe, _ => true, preserveBuild: false, preserveProbe: false, semi: true);

        var rows = Drain(iterator, buildIntColumns: 1);
        Assert.Equal(1, iterator.RowBuffer.Bits32Count);
        Assert.Equal(new object?[] { 2, 3 }, SingleColumn(rows));
    }

    [Fact]
    public void Iterators_EmittedHashMatch_AntiSemi_EmitsUnmatchedBuildRows()
    {
        var buildRows = new object?[] { 1, 2, 3 };
        var probeRows = new object?[] { 2, 3, 4 };

        using var build = new MockedIterator(buildRows);
        using var probe = new MockedIterator(probeRows);

        using var iterator = Join(build, probe, _ => true, preserveBuild: false, preserveProbe: false, semi: false, anti: true);

        var rows = Drain(iterator, buildIntColumns: 1);
        Assert.Equal(1, iterator.RowBuffer.Bits32Count);
        Assert.Equal(new object?[] { 1 }, SingleColumn(rows));
    }

    // A NULL build key never matches, so semi excludes it and anti keeps it.
    [Fact]
    public void Iterators_EmittedHashMatch_AntiSemi_KeepsNullKeyBuildRow()
    {
        var buildRows = new object?[] { 1, null };
        var probeRows = new object?[] { 1, null };

        using var build = new MockedIterator(buildRows);
        using var probe = new MockedIterator(probeRows);

        using var iterator = Join(build, probe, _ => true, preserveBuild: false, preserveProbe: false, semi: false, anti: true);

        var rows = Drain(iterator, buildIntColumns: 1);
        Assert.Equal(new object?[] { null }, SingleColumn(rows));
    }

    // A probing semi (decorrelated EXISTS) emits every build row with a trailing boolean
    // marker reporting whether it matched.
    [Fact]
    public void Iterators_EmittedHashMatch_ProbingSemi_EmitsAllBuildRowsWithMarker()
    {
        var buildRows = new object?[] { 1, 2, 3 };
        var probeRows = new object?[] { 2, 3, 4 };

        using var build = new MockedIterator(buildRows);
        using var probe = new MockedIterator(probeRows);

        using var iterator = Join(build, probe, _ => true, preserveBuild: false, preserveProbe: false, semi: true, anti: false, probing: true);

        iterator.Open();
        Assert.Equal(2, iterator.RowBuffer.Bits32Count);

        var byKey = new Dictionary<int, bool>();
        while (iterator.Read())
            byKey[iterator.RowBuffer.NullableInt32(0)!.Value] = iterator.RowBuffer.Bool(1);

        Assert.False(byKey[1]);
        Assert.True(byKey[2]);
        Assert.True(byKey[3]);
    }

    // Collects every produced row (build columns are ints; a probing semi appends a
    // trailing bool marker). The assertions compare the build column order-independently
    // so they don't pin down the flush order.
    private static List<object?[]> Drain(Iterator iterator, int buildIntColumns)
    {
        var result = new List<object?[]>();
        iterator.Open();
        while (iterator.Read())
        {
            var rb = iterator.RowBuffer;
            var row = new object?[rb.Bits32Count];
            for (var i = 0; i < rb.Bits32Count; i++)
                row[i] = i < buildIntColumns ? rb.NullableInt32(i) : rb.Bool(i);
            result.Add(row);
        }

        return result;
    }

    private static object?[] SingleColumn(List<object?[]> rows)
    {
        return rows.Select(r => r[0]).OrderBy(v => v).ToArray();
    }
}
