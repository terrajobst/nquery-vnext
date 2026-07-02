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

// Compilation only, against the real Northwind schema. Each iteration drives a *fresh* Query
// through ExecuteSchemaReader(): parse -> bind -> algebrize -> optimize -> plan -> emit, but no
// row iteration. The schema-reader path is used deliberately so the measurement is comparable
// across all three engines:
//
//   * Refactored and Old emit eagerly (Compilation.Compile() / IteratorCreator.Convert build
//     the executable plan), so most cost is in the compile step itself.
//   * Baseline defers emit to the *first open*: Compilation.Compile() only binds/plans, and
//     IteratorBuilder.Build runs inside CreateReader(). Stopping at Compile() would therefore
//     undercount Baseline. ExecuteSchemaReader() forces every engine through emit (it builds
//     the iterator tree) without actually reading rows, so all three measure the same phase.
[MemoryDiagnoser]
public class NorthwindCompilationBenchmarks
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

    private string _sql = null!;
    private OldNQuery.DataContext _oldContext = null!;
    private BaselineNQuery.DataContext _baselineContext = null!;
    private CurrentNQuery.Catalog _refactoredCatalog = null!;

    [GlobalSetup]
    public void Setup()
    {
        _sql = NorthwindWorkload.Sql(Shape);
        _oldContext = BuildOldContext();
        _baselineContext = BuildBaselineContext();
        _refactoredCatalog = BuildCurrentCatalog();

        // Warm type-resolution / aggregate-definition caches so steady-state compile is measured
        // rather than first-touch cache population.
        CompileOld();
        CompileBaseline();
        CompileRefactored();
    }

    [Benchmark]
    public int Old() => CompileOld();

    [Benchmark(Baseline = true)]
    public int Baseline() => CompileBaseline();

    [Benchmark]
    public int Refactored() => CompileRefactored();

    private int CompileOld()
    {
        using var reader = new OldNQuery.Query(_sql, _oldContext).ExecuteSchemaReader();
        return reader.FieldCount;
    }

    private int CompileBaseline()
    {
        using var reader = BaselineNQuery.Query.Create(_baselineContext, _sql).ExecuteSchemaReader();
        return reader.ColumnCount;
    }

    private int CompileRefactored()
    {
        using var reader = CurrentNQuery.Query.Create(_refactoredCatalog, _sql).ExecuteSchemaReader();
        return reader.ColumnCount;
    }

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
}
