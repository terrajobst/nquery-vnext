extern alias baseline;

using BenchmarkDotNet.Attributes;

using BaselineNQuery = baseline::NQuery;
using BaselineSymbols = baseline::NQuery.Symbols;
using CurrentNQuery = NQuery;
using CurrentMetadata = NQuery.Metadata;

namespace NQuery.Benchmarks;

// Compilation only: parse is done once in setup; each iteration binds/algebrizes/plans/
// emits. Independent of row count (the table is schema-only), so RowCount is not a param.
[MemoryDiagnoser]
public class CompilationBenchmarks
{
    [Params(Workload.Shape.Scan, Workload.Shape.Aggregate)]
    public Workload.Shape Shape { get; set; }

    private BaselineNQuery.DataContext _oldContext = null!;
    private BaselineNQuery.SyntaxTree _oldSyntax = null!;
    private CurrentNQuery.DataContext _newContext = null!;
    private CurrentNQuery.CodeAnalysis.SyntaxTree _newSyntax = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Compilation only needs the table schema, not data.
        var rows = Workload.BuildRows(0);
        var sql = Workload.Sql(Shape);

        var baselineTable = new BaselineSymbols.SchemaTableSymbol(BaselineSymbols.TableDefinition.Create("Numbers", rows));
        _oldContext = BaselineNQuery.DataContext.Default.AddTables(baselineTable);
        _oldSyntax = BaselineNQuery.SyntaxTree.ParseQuery(sql);

        var currentTable = CurrentMetadata.TableDefinition.Create("Numbers", rows);
        _newContext = CurrentNQuery.DataContext.Default.AddTables(currentTable);
        _newSyntax = CurrentNQuery.CodeAnalysis.SyntaxTree.ParseQuery(sql);

        // Warm aggregate-definition / type-resolution caches so steady-state compile is measured.
        BaselineNQuery.Compilation.Create(_oldContext, _oldSyntax).Compile();
        CurrentNQuery.CodeAnalysis.Compilation.Create(_newContext, _newSyntax).Compile();
    }

    [Benchmark(Baseline = true)]
    public object Old() => BaselineNQuery.Compilation.Create(_oldContext, _oldSyntax).Compile();

    [Benchmark]
    public object New() => CurrentNQuery.CodeAnalysis.Compilation.Create(_newContext, _newSyntax).Compile();
}
