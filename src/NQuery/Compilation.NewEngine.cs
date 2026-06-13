#if !BASELINE

using System.Collections.Immutable;

using NQuery.Planning;
using NQuery.Refactor.Algebra;
using NQuery.Refactor.Emit;
using NQuery.Refactor.Optimization;
using NQuery.Syntax;

namespace NQuery
{
    // The new query pipeline: Bind -> Algebrize -> Optimize -> Plan -> Emit, mirroring
    // the sequence the differential tests drive. Compiled into every non-BASELINE build
    // (i.e. the live NQuery assembly and the benchmark's "current" engine); the BASELINE
    // build keeps using the legacy Binder/Optimizer/IteratorBuilder path in Compilation.cs.
    partial class Compilation
    {
        private CompiledQuery CompileNewEngine()
        {
            // The new pipeline only handles top-level queries. Bare expressions
            // (Expression<T>, e.g. aggregate type resolution) keep using the legacy
            // engine -- its binder/diagnostics are what callers like the aggregate
            // definitions depend on.
            if (SyntaxTree.Root.Root is not QuerySyntax)
                return CompileLegacy();

            var bindingResult = Refactor.Binding.Binder.Bind(SyntaxTree.Root, DataContext);

            var diagnostics = SyntaxTree.GetDiagnostics()
                                        .Concat(bindingResult.Diagnostics)
                                        .ToImmutableArray();
            if (diagnostics.Any())
                throw new CompilationException(diagnostics);

            var boundQuery = (Refactor.Binding.BoundQuery)bindingResult.BoundRoot;
            var logicalQuery = LogicalOptimizer.Optimize(Algebrizer.Algebrize(boundQuery));
            var physicalQuery = Planner.Plan(logicalQuery);
            var plan = Emitter.Emit(physicalQuery);

            return new CompiledQuery(plan);
        }
    }
}

#endif
