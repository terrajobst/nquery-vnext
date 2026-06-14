using NQuery.Iterators;

namespace NQuery.Tests.Refactor.Iterators;

// The legacy BoundHashMatchOperator maps to two booleans:
//   Inner       = (preserveBuild: false, preserveProbe: false)
//   LeftOuter   = (preserveBuild: true,  preserveProbe: false)
//   RightOuter  = (preserveBuild: false, preserveProbe: true)
//   FullOuter   = (preserveBuild: true,  preserveProbe: true)
// The remainder predicate receives the combined (build ++ probe) buffer.
public class EmittedHashMatchIteratorTests : IteratorTests
{
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

        using (var iterator = new EmittedHashMatchIterator(build, probe, 0, 0, _ => true, preserveBuild, preserveProbe))
        {
            for (var i = 0; i < passCount; i++)
            {
                iterator.Open();

                Assert.True(iterator.Read());
                Assert.Equal(2, iterator.RowBuffer[0]);
                Assert.Equal(2, iterator.RowBuffer[1]);

                if (preserveProbe)
                {
                    Assert.True(iterator.Read());
                    Assert.Null(iterator.RowBuffer[0]);
                    Assert.Equal(3, iterator.RowBuffer[1]);
                }

                if (preserveBuild)
                {
                    Assert.True(iterator.Read());
                    Assert.Equal(1, iterator.RowBuffer[0]);
                    Assert.Null(iterator.RowBuffer[1]);
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
        var buildRows = Array.Empty<object>();
        var probeRows = new object?[] { 2, 3 };

        using var build = new MockedIterator(buildRows);
        using var probe = new MockedIterator(probeRows);

        using var iterator = new EmittedHashMatchIterator(build, probe, 0, 0, _ => true, preserveBuild, preserveProbe);
        AssertEmpty(iterator);
    }

    [Theory]
    [InlineData(false, true)] // RightOuter
    [InlineData(true, true)]  // FullOuter
    public void Iterators_EmittedHashMatch_ReturnsProbe_IfBuildIsEmpty(bool preserveBuild, bool preserveProbe)
    {
        var buildRows = Array.Empty<object>();
        var probeRows = new object?[] { 2, 3 };
        var expected = new object?[,]
        {
            {null, 2},
            {null, 3}
        };

        using var build = new MockedIterator(buildRows);
        using var probe = new MockedIterator(probeRows);

        using var iterator = new EmittedHashMatchIterator(build, probe, 0, 0, _ => true, preserveBuild, preserveProbe);
        AssertProduces(iterator, expected);
    }

    [Theory]
    [InlineData(false, false)] // Inner
    [InlineData(false, true)]  // RightOuter
    public void Iterators_EmittedHashMatch_ReturnsEmpty_IfProbeIsEmpty(bool preserveBuild, bool preserveProbe)
    {
        var buildRows = new object?[] { 1, 2 };
        var probeRows = Array.Empty<object>();

        using var build = new MockedIterator(buildRows);
        using var probe = new MockedIterator(probeRows);

        using var iterator = new EmittedHashMatchIterator(build, probe, 0, 0, _ => true, preserveBuild, preserveProbe);
        AssertEmpty(iterator);
    }

    [Theory]
    [InlineData(true, false)] // LeftOuter
    [InlineData(true, true)]  // FullOuter
    public void Iterators_EmittedHashMatch_ReturnsBuild_IfProbeIsEmpty(bool preserveBuild, bool preserveProbe)
    {
        var buildRows = new object?[] { 1, 2 };
        var probeRows = Array.Empty<object>();
        var expected = new object?[,]
        {
            {1, null},
            {2, null}
        };

        using var build = new MockedIterator(buildRows);
        using var probe = new MockedIterator(probeRows);

        using var iterator = new EmittedHashMatchIterator(build, probe, 0, 0, _ => true, preserveBuild, preserveProbe);
        AssertProduces(iterator, expected);
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

        using var build = new MockedIterator(buildRows);
        using var probe = new MockedIterator(probeRows);

        using var iterator = new EmittedHashMatchIterator(build, probe, 0, 0, _ => true, preserveBuild, preserveProbe);
        iterator.Open();

        if (preserveProbe)
        {
            Assert.True(iterator.Read());
            Assert.Null(iterator.RowBuffer[0]);
            Assert.Null(iterator.RowBuffer[1]);
            Assert.Null(iterator.RowBuffer[2]);
            Assert.Equal("Bar", iterator.RowBuffer[3]);
        }

        if (preserveBuild)
        {
            Assert.True(iterator.Read());
            Assert.Null(iterator.RowBuffer[0]);
            Assert.Equal("Foo", iterator.RowBuffer[1]);
            Assert.Null(iterator.RowBuffer[2]);
            Assert.Null(iterator.RowBuffer[3]);
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

        using var iterator = new EmittedHashMatchIterator(build, probe, 0, 0, _ => true, preserveBuild, preserveProbe);
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

        // Layout:
        // 0               | 1            | 2               | 3            | 4
        // ----------------+--------------+-----------------+--------------+---------------
        // build.ProjectId | build.TaskId | probe.ProjectId | probe.TaskId | probe.TaskName
        var remainder = new EmittedPredicate(rb => Equals(rb[1], rb[3]));

        using var iterator = new EmittedHashMatchIterator(build, probe, 0, 0, remainder, preserveBuild, preserveProbe);
        iterator.Open();

        if (preserveProbe)
        {
            Assert.True(iterator.Read());
            Assert.Null(iterator.RowBuffer[0]);
            Assert.Null(iterator.RowBuffer[1]);
            Assert.Equal(0, iterator.RowBuffer[2]);
            Assert.Equal(0, iterator.RowBuffer[3]);
            Assert.Equal("Unmatched1", iterator.RowBuffer[4]);
        }

        Assert.True(iterator.Read());
        Assert.Equal(1, iterator.RowBuffer[0]);
        Assert.Equal(1, iterator.RowBuffer[1]);
        Assert.Equal(1, iterator.RowBuffer[2]);
        Assert.Equal(1, iterator.RowBuffer[3]);
        Assert.Equal("Project1-Task-1", iterator.RowBuffer[4]);

        Assert.True(iterator.Read());
        Assert.Equal(1, iterator.RowBuffer[0]);
        Assert.Equal(2, iterator.RowBuffer[1]);
        Assert.Equal(1, iterator.RowBuffer[2]);
        Assert.Equal(2, iterator.RowBuffer[3]);
        Assert.Equal("Project1-Task-2", iterator.RowBuffer[4]);

        if (preserveProbe)
        {
            Assert.True(iterator.Read());
            Assert.Null(iterator.RowBuffer[0]);
            Assert.Null(iterator.RowBuffer[1]);
            Assert.Equal(1, iterator.RowBuffer[2]);
            Assert.Equal(3, iterator.RowBuffer[3]);
            Assert.Equal("Unmatched2", iterator.RowBuffer[4]);
        }

        Assert.True(iterator.Read());
        Assert.Equal(2, iterator.RowBuffer[0]);
        Assert.Equal(1, iterator.RowBuffer[1]);
        Assert.Equal(2, iterator.RowBuffer[2]);
        Assert.Equal(1, iterator.RowBuffer[3]);
        Assert.Equal("Project2-Task-1", iterator.RowBuffer[4]);

        if (preserveBuild)
        {
            Assert.True(iterator.Read());
            Assert.Equal(2, iterator.RowBuffer[0]);
            Assert.Equal(2, iterator.RowBuffer[1]);
            Assert.Null(iterator.RowBuffer[2]);
            Assert.Null(iterator.RowBuffer[3]);
            Assert.Null(iterator.RowBuffer[4]);

            Assert.True(iterator.Read());
            Assert.Equal(3, iterator.RowBuffer[0]);
            Assert.Equal(1, iterator.RowBuffer[1]);
            Assert.Null(iterator.RowBuffer[2]);
            Assert.Null(iterator.RowBuffer[3]);
            Assert.Null(iterator.RowBuffer[4]);
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

        using var iterator = new EmittedHashMatchIterator(build, probe, 0, 0, _ => true, preserveBuild: false, preserveProbe: false, semi: true);

        var rows = Drain(iterator);
        Assert.Equal(1, iterator.RowBuffer.Count);
        Assert.Equal(new object[] { 2, 3 }, SingleColumn(rows));
    }

    [Fact]
    public void Iterators_EmittedHashMatch_AntiSemi_EmitsUnmatchedBuildRows()
    {
        var buildRows = new object?[] { 1, 2, 3 };
        var probeRows = new object?[] { 2, 3, 4 };

        using var build = new MockedIterator(buildRows);
        using var probe = new MockedIterator(probeRows);

        using var iterator = new EmittedHashMatchIterator(build, probe, 0, 0, _ => true, preserveBuild: false, preserveProbe: false, semi: false, anti: true);

        var rows = Drain(iterator);
        Assert.Equal(1, iterator.RowBuffer.Count);
        Assert.Equal(new object[] { 1 }, SingleColumn(rows));
    }

    // A NULL build key never matches, so semi excludes it and anti keeps it.
    [Fact]
    public void Iterators_EmittedHashMatch_AntiSemi_KeepsNullKeyBuildRow()
    {
        var buildRows = new object?[] { 1, null };
        var probeRows = new object?[] { 1, null };

        using var build = new MockedIterator(buildRows);
        using var probe = new MockedIterator(probeRows);

        using var iterator = new EmittedHashMatchIterator(build, probe, 0, 0, _ => true, preserveBuild: false, preserveProbe: false, semi: false, anti: true);

        var rows = Drain(iterator);
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

        using var iterator = new EmittedHashMatchIterator(build, probe, 0, 0, _ => true, preserveBuild: false, preserveProbe: false, semi: true, anti: false, probing: true);

        var rows = Drain(iterator);
        Assert.Equal(2, iterator.RowBuffer.Count);

        var byKey = rows.ToDictionary(r => r[0], r => r[1]);
        Assert.Equal(false, byKey[1]);
        Assert.Equal(true, byKey[2]);
        Assert.Equal(true, byKey[3]);
    }

    // Collects every produced row; the assertions compare the build column
    // order-independently so they don't pin down the flush order.
    private static List<object[]> Drain(Iterator iterator)
    {
        var result = new List<object[]>();
        iterator.Open();
        while (iterator.Read())
        {
            var row = new object[iterator.RowBuffer.Count];
            for (var i = 0; i < row.Length; i++)
                row[i] = iterator.RowBuffer[i];
            result.Add(row);
        }

        return result;
    }

    private static object[] SingleColumn(List<object[]> rows)
    {
        return rows.Select(r => r[0]).OrderBy(v => v).ToArray();
    }
}
