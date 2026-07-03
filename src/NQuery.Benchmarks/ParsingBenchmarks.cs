extern alias baseline;
extern alias old;

using BenchmarkDotNet.Attributes;

using BaselineNQuery = baseline::NQuery;
using CurrentNQuery = NQuery;
using OldNQuery = old::NQuery;

namespace NQuery.Benchmarks;

// Parse only: each iteration lexes and parses the SQL into a syntax tree and nothing else.
// Parsing is schema-independent, so no context/catalog is built -- all three engines run the
// exact same input through their parser. The old engine's Parser type is internal, so it is
// reached through a tiny public shim (NQuery.BenchmarkParser) added to that engine; baseline and
// refactored both expose a public SyntaxTree.ParseQuery.
[MemoryDiagnoser]
public class ParsingBenchmarks
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

    [GlobalSetup]
    public void Setup()
    {
        _sql = NorthwindWorkload.Sql(Shape);

        // Touch each parser once so first-use JIT / static initialization is out of the measurement.
        ParseOld();
        ParseBaseline();
        ParseRefactored();
    }

    [Benchmark]
    public object Old() => ParseOld();

    [Benchmark(Baseline = true)]
    public object Baseline() => ParseBaseline();

    [Benchmark]
    public object Refactored() => ParseRefactored();

    private object ParseOld() => OldNQuery.BenchmarkParser.ParseQuery(_sql);

    private object ParseBaseline() => BaselineNQuery.SyntaxTree.ParseQuery(_sql);

    private object ParseRefactored() => CurrentNQuery.CodeAnalysis.SyntaxTree.ParseQuery(_sql);
}
