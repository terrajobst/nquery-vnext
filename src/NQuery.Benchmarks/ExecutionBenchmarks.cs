extern alias baseline;

using BenchmarkDotNet.Attributes;
using NQuery.CodeAnalysis;

using BaselineNQuery = baseline::NQuery;
using BaselineSymbols = baseline::NQuery.Symbols;
using CurrentNQuery = NQuery;
using CurrentMetadata = NQuery.Metadata;

namespace NQuery.Benchmarks;

// Execution only: the query is compiled once in setup, then drained per iteration.
// Compilation cost is excluded, so this measures the per-row engine cost (and its
// allocations) in isolation.
[MemoryDiagnoser]
public class ExecutionBenchmarks
{
    [Params(1_000, 10_000, 100_000)]
    public int RowCount { get; set; }

    [Params(Workload.Shape.Scan, Workload.Shape.Aggregate)]
    public Workload.Shape Shape { get; set; }

    private BaselineNQuery.Query _old = null!;
    private CurrentNQuery.Query _new = null!;

    [GlobalSetup]
    public void Setup()
    {
        var rows = Workload.BuildRows(RowCount);
        var sql = Workload.Sql(Shape);

        var baselineTable = new BaselineSymbols.SchemaTableSymbol(BaselineSymbols.TableDefinition.Create("Numbers", rows));
        _old = BaselineNQuery.Query.Create(BaselineNQuery.DataContext.Default.AddTables(baselineTable), sql);

        var currentTable = CurrentMetadata.TableDefinition.Create("Numbers", rows);
        _new = CurrentNQuery.Query.Create(CurrentNQuery.DataContext.Default.AddTables(currentTable), sql);

        // First read compiles and caches the plan on the Query; the benchmarks below reuse it.
        Drain(_old.ExecuteReader());
        Drain(_new.ExecuteReader());
    }

    [Benchmark(Baseline = true)]
    public int Old() => Drain(_old.ExecuteReader());

    [Benchmark]
    public int New() => Drain(_new.ExecuteReader());

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
