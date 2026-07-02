extern alias baseline;

using BenchmarkDotNet.Attributes;

using BaselineNQuery = baseline::NQuery;
using BaselineSymbols = baseline::NQuery.Symbols;
using CurrentMetadata = NQuery.Metadata;
using CurrentNQuery = NQuery;

namespace NQuery.Benchmarks;

// Reader preparation: a fresh Query is compiled and a reader opened every iteration, but
// no rows are read. ExecuteReader() does all the codegen while Open() on a (blocking)
// sort does not yet sort. This avoids distorting the measurement: CompilationBenchmarks
// defers codegen to ExecuteReader, and reading the first row would drag a blocking sort's
// full execution into the Aggregate measurement.
[MemoryDiagnoser]
public class ReaderPreparationBenchmarks
{
    private const int RowCount = 1_000;

    [Params(Workload.Shape.Scan, Workload.Shape.Aggregate)]
    public Workload.Shape Shape { get; set; }

    private BaselineNQuery.DataContext _oldContext = null!;
    private CurrentNQuery.Catalog _newContext = null!;
    private string _sql = null!;

    [GlobalSetup]
    public void Setup()
    {
        var rows = Workload.BuildRows(RowCount);
        _sql = Workload.Sql(Shape);

        var baselineTable = new BaselineSymbols.SchemaTableSymbol(BaselineSymbols.TableDefinition.Create("Numbers", rows));
        _oldContext = BaselineNQuery.DataContext.Default.AddTables(baselineTable);

        var currentTable = CurrentMetadata.TableDefinition.Create("Numbers", rows);
        _newContext = CurrentNQuery.Catalog.Default.AddTables(currentTable);
    }

    [Benchmark(Baseline = true)]
    public int Old()
    {
        // Fresh Query -> compiles (bind + optimize) then builds iterators and compiles
        // expressions in CreateReader; no rows read.
        var query = BaselineNQuery.Query.Create(_oldContext, _sql);
        using var reader = query.ExecuteReader();
        return reader.ColumnCount;
    }

    [Benchmark]
    public int New()
    {
        // Fresh Query -> compiles (bind + algebrize + optimize + plan + emit, the last
        // eagerly compiling delegates); no rows read.
        var query = CurrentNQuery.Query.Create(_newContext, _sql);
        using var reader = query.ExecuteReader();
        return reader.ColumnCount;
    }
}
