using BenchmarkDotNet.Running;

using NQuery.Benchmarks;

// `dotnet run -c Release -- validate` runs the differential correctness gate instead of
// the benchmarks: same query, both engines, rows compared. It needs the comparison engines from
// the submodules under external/, which the project file compiles out when they are not built.
#if HAS_OLD_ENGINES
if (args is ["validate", ..])
    return Validator.Run();
#else
if (args is ["validate", ..])
{
    Console.Error.WriteLine("validate needs the engines in external/; see NQuery.Benchmarks.csproj.");
    return 1;
}
#endif

// With no args, run every benchmark instead of dropping into the interactive switcher
// (which would block waiting on stdin).
if (args.Length == 0)
    args = ["--filter", "*"];

BenchmarkSwitcher.FromAssembly(typeof(Workload).Assembly).Run(args);
return 0;
