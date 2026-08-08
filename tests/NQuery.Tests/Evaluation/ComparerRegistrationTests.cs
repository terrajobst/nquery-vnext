using System.Collections;

using NQuery.Metadata;

namespace NQuery.Tests.Evaluation;

public class ComparerRegistrationTests : EvaluationTest
{
    private sealed class NumberRow
    {
        public NumberRow(int value)
        {
            Value = value;
        }

        public int Value { get; }
    }

    // Implements only IComparer<int>, so it exercises both the generic registration overload and
    // the typed (unboxed) sort path.
    private sealed class DescendingInt32Comparer : IComparer<int>
    {
        public int Compare(int x, int y)
        {
            return y.CompareTo(x);
        }
    }

    // Implements only the non-generic IComparer, the path that still boxes.
    private sealed class DescendingComparer : IComparer
    {
        public int Compare(object? x, object? y)
        {
            return Comparer<int>.Default.Compare((int)y!, (int)x!);
        }
    }

    [Fact]
    public void Evaluation_OrderBy_HonorsRegisteredGenericComparer()
    {
        var rows = new[] { new NumberRow(1), new NumberRow(2), new NumberRow(3) };
        var table = TableDefinition.Create("Numbers", rows);
        var catalog = Catalog.Empty.AddTables(table).AddComparer<int>(new DescendingInt32Comparer());

        var text = @"SELECT n.Value FROM Numbers n ORDER BY n.Value";

        AssertProduces(text, [3, 2, 1], catalog);
    }

    [Fact]
    public void Evaluation_OrderBy_HonorsRegisteredNonGenericComparer()
    {
        var rows = new[] { new NumberRow(1), new NumberRow(2), new NumberRow(3) };
        var table = TableDefinition.Create("Numbers", rows);
        var catalog = Catalog.Empty.AddTables(table).AddComparer(typeof(int), new DescendingComparer());

        var text = @"SELECT n.Value FROM Numbers n ORDER BY n.Value";

        AssertProduces(text, [3, 2, 1], catalog);
    }
}
