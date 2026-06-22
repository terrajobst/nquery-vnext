extern alias baseline;

using NQuery.Northwind;

using BaselineNQuery = baseline::NQuery;
using BaselineSymbols = baseline::NQuery.Symbols;
using CurrentNQuery = NQuery;
using CurrentMetadata = NQuery.Metadata;

namespace NQuery.Benchmarks;

// Differential correctness gate: runs each query through both engines via the public
// Query API (the exact path the benchmarks measure) and compares the result rows. Run
// with `dotnet run -c Release -- validate` before trusting any timings.
internal static class Validator
{
    private static readonly string[] Queries =
    [
        "SELECT Id, Value FROM Numbers WHERE Id < 10",
        "SELECT Category, COUNT(*), SUM(Value) FROM Numbers WHERE Value > 100.0 GROUP BY Category",
        "SELECT Id, Value FROM Numbers WHERE Value > 200.0 ORDER BY Value, Id",
    ];

    public static int Run()
    {
        var failures = 0;

        var rows = Workload.BuildRows(1_000);

        var baselineTable = new BaselineSymbols.SchemaTableSymbol(BaselineSymbols.TableDefinition.Create("Numbers", rows));
        var baselineContext = BaselineNQuery.DataContext.Default.AddTables(baselineTable);

        var currentTable = CurrentMetadata.TableDefinition.Create("Numbers", rows);
        var currentContext = CurrentNQuery.Catalog.Default.AddTables(currentTable);

        failures += Validate(Queries, baselineContext, currentContext);
        failures += Validate(NorthwindQueries, BuildNorthwindBaseline(), BuildNorthwindCurrent());

        Console.WriteLine();
        Console.WriteLine(failures == 0 ? "All queries match." : $"{failures} mismatch(es).");
        return failures == 0 ? 0 : 1;
    }

    private static int Validate(IEnumerable<string> queries, BaselineNQuery.DataContext baselineContext, CurrentNQuery.Catalog currentContext)
    {
        var failures = 0;

        foreach (var sql in queries)
        {
            var expected = RunBaseline(baselineContext, sql);
            var actual = RunCurrent(currentContext, sql);

            if (RowsEqual(expected, actual))
            {
                Console.WriteLine($"PASS ({expected.Count} rows): {sql}");
            }
            else
            {
                failures++;
                Console.WriteLine($"FAIL: {sql}");
                Console.WriteLine($"  baseline: {expected.Count} rows");
                Console.WriteLine($"  current : {actual.Count} rows");
                DumpFirstDifference(expected, actual);
            }
        }

        return failures;
    }

    private static readonly string[] NorthwindQueries =
        Enum.GetValues<NorthwindWorkload.Shape>().Select(NorthwindWorkload.Sql).ToArray();

    private static BaselineNQuery.DataContext BuildNorthwindBaseline()
    {
        var data = NorthwindData.Instance;
        return BaselineNQuery.DataContext.Default.AddTables(
            new BaselineSymbols.SchemaTableSymbol(BaselineSymbols.TableDefinition.Create("Customers", data.Customers)),
            new BaselineSymbols.SchemaTableSymbol(BaselineSymbols.TableDefinition.Create("Orders", data.Orders)),
            new BaselineSymbols.SchemaTableSymbol(BaselineSymbols.TableDefinition.Create("Order Details", data.OrderDetails)));
    }

    private static CurrentNQuery.Catalog BuildNorthwindCurrent()
    {
        var data = NorthwindData.Instance;
        return CurrentNQuery.Catalog.Default.AddTables(
            CurrentMetadata.TableDefinition.Create("Customers", data.Customers),
            CurrentMetadata.TableDefinition.Create("Orders", data.Orders),
            CurrentMetadata.TableDefinition.Create("Order Details", data.OrderDetails));
    }

    private static List<object?[]> RunBaseline(BaselineNQuery.DataContext context, string sql)
    {
        using var reader = BaselineNQuery.Query.Create(context, sql).ExecuteReader();
        var rows = new List<object?[]>();
        while (reader.Read())
        {
            var row = new object?[reader.ColumnCount];
            for (var i = 0; i < row.Length; i++)
                row[i] = reader[i];
            rows.Add(row);
        }
        return rows;
    }

    private static List<object?[]> RunCurrent(CurrentNQuery.Catalog context, string sql)
    {
        using var reader = CurrentNQuery.Query.Create(context, sql).ExecuteReader();
        var rows = new List<object?[]>();
        while (reader.Read())
        {
            var row = new object?[reader.ColumnCount];
            for (var i = 0; i < row.Length; i++)
                row[i] = reader[i];
            rows.Add(row);
        }
        return rows;
    }

    // Order-insensitive comparison: the two engines need not emit grouped rows in the
    // same order unless the query has an ORDER BY.
    private static bool RowsEqual(List<object?[]> a, List<object?[]> b)
    {
        if (a.Count != b.Count)
            return false;

        var sa = a.Select(Key).OrderBy(s => s, StringComparer.Ordinal).ToList();
        var sb = b.Select(Key).OrderBy(s => s, StringComparer.Ordinal).ToList();
        return sa.SequenceEqual(sb);
    }

    private static string Key(object?[] row)
    {
        return string.Join("|", row.Select(v => v?.ToString() ?? "<null>"));
    }

    private static void DumpFirstDifference(List<object?[]> expected, List<object?[]> actual)
    {
        var e = expected.Select(Key).OrderBy(s => s, StringComparer.Ordinal).ToList();
        var a = actual.Select(Key).OrderBy(s => s, StringComparer.Ordinal).ToList();
        var max = Math.Min(e.Count, a.Count);
        for (var i = 0; i < max; i++)
        {
            if (e[i] != a[i])
            {
                Console.WriteLine($"  first diff @ row {i}:");
                Console.WriteLine($"    baseline: {e[i]}");
                Console.WriteLine($"    current : {a[i]}");
                return;
            }
        }
    }
}
