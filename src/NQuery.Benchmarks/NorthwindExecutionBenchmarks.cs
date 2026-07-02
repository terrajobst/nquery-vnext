extern alias baseline;
extern alias old;

using BenchmarkDotNet.Attributes;

using NQuery.Northwind;

using BaselineNQuery = baseline::NQuery;
using BaselineSymbols = baseline::NQuery.Symbols;
using CurrentMetadata = NQuery.Metadata;
using CurrentNQuery = NQuery;
using OldNQuery = old::NQuery;

namespace NQuery.Benchmarks;

// Execution only, against the real Northwind schema: each query is compiled once in setup,
// then drained per iteration. Compilation cost is excluded, so this measures per-row engine
// cost (and allocations) in isolation. All three engines -- the original (nquery-old), the
// baseline (main), and the refactored (this worktree) -- are fed the exact same strongly typed
// records (see NorthwindWorkload), so any difference is the row-buffer representation.
[MemoryDiagnoser]
public class NorthwindExecutionBenchmarks
{
    [Params(
        NorthwindWorkload.Shape.Scan,
        NorthwindWorkload.Shape.Join,
        NorthwindWorkload.Shape.Aggregate,
        NorthwindWorkload.Shape.Sort,
        NorthwindWorkload.Shape.Report,
        NorthwindWorkload.Shape.TopWithTies,
        NorthwindWorkload.Shape.Decorrelated,
        NorthwindWorkload.Shape.NestedLoops)]
    public NorthwindWorkload.Shape Shape { get; set; }

    private OldNQuery.Query _old = null!;
    private BaselineNQuery.Query _baseline = null!;
    private CurrentNQuery.Query _refactored = null!;

    [GlobalSetup]
    public void Setup()
    {
        var sql = NorthwindWorkload.Sql(Shape);

        _old = new OldNQuery.Query(sql, BuildOldContext());
        _baseline = BaselineNQuery.Query.Create(BuildBaselineContext(), sql);
        _refactored = CurrentNQuery.Query.Create(BuildCurrentCatalog(), sql);

        // First read compiles and caches the plan on the Query; the benchmarks below reuse it.
        Drain(_old.ExecuteDataReader());
        Drain(_baseline.ExecuteReader());
        Drain(_refactored.ExecuteReader());
    }

    // The original engine (main is the comparison reference for ratios).
    [Benchmark]
    public int Old() => Drain(_old.ExecuteDataReader());

    [Benchmark(Baseline = true)]
    public int Baseline() => Drain(_baseline.ExecuteReader());

    [Benchmark]
    public int Refactored() => Drain(_refactored.ExecuteReader());

    private static OldNQuery.DataContext BuildOldContext()
    {
        var data = NorthwindData.Instance;
        var context = new OldNQuery.DataContext();
        context.Tables.Add(data.Customers, "Customers");
        context.Tables.Add(data.Orders, "Orders");
        context.Tables.Add(data.OrderDetails, "Order Details");
        return context;
    }

    private static BaselineNQuery.DataContext BuildBaselineContext()
    {
        var data = NorthwindData.Instance;
        return BaselineNQuery.DataContext.Default.AddTables(
            new BaselineSymbols.SchemaTableSymbol(BaselineSymbols.TableDefinition.Create("Customers", data.Customers)),
            new BaselineSymbols.SchemaTableSymbol(BaselineSymbols.TableDefinition.Create("Orders", data.Orders)),
            new BaselineSymbols.SchemaTableSymbol(BaselineSymbols.TableDefinition.Create("Order Details", data.OrderDetails)));
    }

    private static CurrentNQuery.Catalog BuildCurrentCatalog()
    {
        var data = NorthwindData.Instance;
        return CurrentNQuery.Catalog.Default.AddTables(
            CurrentMetadata.TableDefinition.Create("Customers", data.Customers),
            CurrentMetadata.TableDefinition.Create("Orders", data.Orders),
            CurrentMetadata.TableDefinition.Create("Order Details", data.OrderDetails));
    }

    private static int Drain(OldNQuery.QueryDataReader reader)
    {
        using (reader)
        {
            var rows = 0;
            while (reader.Read())
                rows++;
            return rows;
        }
    }

    private static int Drain(BaselineNQuery.QueryReader reader)
    {
        using (reader)
        {
            var rows = 0;
            while (reader.Read())
                rows++;
            return rows;
        }
    }

    private static int Drain(CurrentNQuery.QueryReader reader)
    {
        using (reader)
        {
            var rows = 0;
            while (reader.Read())
                rows++;
            return rows;
        }
    }
}
