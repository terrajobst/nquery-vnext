using BenchmarkDotNet.Running;

using NQuery.Benchmarks;

// `dotnet run -c Release -- validate` runs the differential correctness gate instead of
// the benchmarks: same query, both engines, rows compared.
if (args is ["validate", ..])
    return Validator.Run();

// With no args, run every benchmark instead of dropping into the interactive switcher
// (which would block waiting on stdin).
if (args.Length == 0)
    args = ["--filter", "*"];

BenchmarkSwitcher.FromAssembly(typeof(Workload).Assembly).Run(args);
return 0;
