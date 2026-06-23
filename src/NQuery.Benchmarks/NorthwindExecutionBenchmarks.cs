extern alias baseline;

using BenchmarkDotNet.Attributes;

using NQuery.Northwind;

using BaselineNQuery = baseline::NQuery;
using BaselineSymbols = baseline::NQuery.Symbols;
using CurrentNQuery = NQuery;
using CurrentMetadata = NQuery.Metadata;

namespace NQuery.Benchmarks;

// Execution only, against the real Northwind schema: each query is compiled once in setup,
// then drained per iteration. Compilation cost is excluded, so this measures per-row engine
// cost (and allocations) in isolation. Both engines are fed the exact same strongly typed
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

    private BaselineNQuery.Query _old = null!;
    private CurrentNQuery.Query _new = null!;

    [GlobalSetup]
    public void Setup()
    {
        var sql = NorthwindWorkload.Sql(Shape);

        _old = BaselineNQuery.Query.Create(BuildBaselineContext(), sql);
        _new = CurrentNQuery.Query.Create(BuildCurrentCatalog(), sql);

        // First read compiles and caches the plan on the Query; the benchmarks below reuse it.
        Drain(_old.ExecuteReader());
        Drain(_new.ExecuteReader());
    }

    [Benchmark(Baseline = true)]
    public int Old() => Drain(_old.ExecuteReader());

    [Benchmark]
    public int New() => Drain(_new.ExecuteReader());

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
