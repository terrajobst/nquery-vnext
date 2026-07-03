using BenchmarkDotNet.Attributes;

namespace NQuery.Benchmarks;

// Recursive-CTE execution regression benchmark: a hierarchy walk over deep vs. wide forests of
// equal total node count (see HierarchyWorkload). The recursive step's base hash is built once
// and reused across rounds (HashMatchIterator's build-once path), so deep and wide should land
// close together and scale ~linearly in node count. The query is compiled once in setup, then
// drained per iteration, so this measures execution only.
[MemoryDiagnoser]
public class HierarchyExecutionBenchmarks
{
    // Total nodes = Forests * 99: 99, 990, 9_900.
    [Params(1, 10, 100)]
    public int Forests { get; set; }

    [Params(HierarchyWorkload.Shape.Deep, HierarchyWorkload.Shape.Wide)]
    public HierarchyWorkload.Shape Shape { get; set; }

    private Query _query = null!;

    [GlobalSetup]
    public void Setup()
    {
        var rows = HierarchyWorkload.BuildRows(Shape, Forests);
        var catalog = Catalog.Default.AddTables(Metadata.TableDefinition.Create("Nodes", rows));
        _query = Query.Create(catalog, HierarchyWorkload.Sql);

        // First read compiles and caches the plan on the Query; the benchmark reuses it.
        Drain();
    }

    [Benchmark]
    public int Walk() => Drain();

    private int Drain()
    {
        using var reader = _query.ExecuteReader();
        var rows = 0;
        while (reader.Read())
            rows++;
        return rows;
    }
}
