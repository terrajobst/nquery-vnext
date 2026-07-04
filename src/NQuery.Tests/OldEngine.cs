extern alias baseline;
// Type aliases (not `using baseline::NQuery;`) on purpose: this file's namespace is nested
// under NQuery, so the enclosing -- live -- NQuery.* types would shadow a namespace import and
// we'd silently run the new engine. Qualifying through these aliases keeps everything baseline.
using BaselineContext = baseline::NQuery.DataContext;
using BaselineData = baseline::NQuery.Data.DataContextExtensions;
using BaselineQuery = baseline::NQuery.Query;

namespace NQuery.Tests;

// Test harness for running queries through the OLD engine (the nquery-baseline worktree,
// aliased "baseline"). It builds its Catalog from the same engine-neutral Northwind
// DataSet the new-engine tests use (NorthwindCatalog), so results are directly
// comparable. The extern alias is confined to this file: callers pass a query string and
// get back a List<object[]>, all standard types, so they never touch baseline:: at all.
internal static class OldEngine
{
    private static readonly BaselineContext Northwind =
        BaselineData.AddTablesAndRelations(BaselineContext.Default, global::NQuery.Northwind.NorthwindCatalog.CreateDataSet());

    public static List<object[]> RunQuery(string text)
    {
        using var reader = BaselineQuery.Create(Northwind, text).ExecuteReader();

        var rows = new List<object[]>();
        while (reader.Read())
        {
            var row = new object[reader.ColumnCount];
            for (var i = 0; i < row.Length; i++)
                row[i] = reader[i];
            rows.Add(row);
        }

        return rows;
    }
}
