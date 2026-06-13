#if !BASELINE

using System.Collections.Immutable;

using NQuery.Planning;
using NQuery.Refactor.Algebra;
using NQuery.Refactor.Emit;
using NQuery.Refactor.Optimization;

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
            var bindingResult = Refactor.Binding.Binder.Bind(SyntaxTree.Root, DataContext);

            var diagnostics = SyntaxTree.GetDiagnostics()
                                        .Concat(bindingResult.Diagnostics)
                                        .ToImmutableArray();
            if (diagnostics.Any())
                throw new CompilationException(diagnostics);

            // A top-level query binds to a BoundQuery; a bare expression (Expression<T>, e.g.
            // aggregate type resolution) binds to a BoundExpression. The algebrizer wraps the
            // latter in a one-row projection so both feed the same Optimize/Plan/Emit pipeline.
            var logicalQuery = bindingResult.BoundRoot switch
            {
                Refactor.Binding.BoundQuery boundQuery => Algebrizer.Algebrize(boundQuery),
                Refactor.Binding.BoundExpression boundExpression => Algebrizer.Algebrize(boundExpression),
                _ => throw ExceptionBuilder.UnexpectedValue(bindingResult.BoundRoot)
            };

            logicalQuery = LogicalOptimizer.Optimize(logicalQuery, DataContext);
            var physicalQuery = Planner.Plan(logicalQuery);
            var plan = Emitter.Emit(physicalQuery);

            return new CompiledQuery(plan);
        }

        // The new-pipeline show plan, mirroring CompileNewEngine's stages: the algebrized
        // (unoptimized) logical tree, each logical optimization pass that changed it, the
        // optimized logical tree, and finally the physical plan the planner produced.
        private IEnumerable<ShowPlan> GetShowPlanStepsNewEngine()
        {
            var bindingResult = Refactor.Binding.Binder.Bind(SyntaxTree.Root, DataContext);

            if (SyntaxTree.GetDiagnostics().Concat(bindingResult.Diagnostics).Any())
                yield break;

            var logicalQuery = bindingResult.BoundRoot switch
            {
                Refactor.Binding.BoundQuery boundQuery => Algebrizer.Algebrize(boundQuery),
                Refactor.Binding.BoundExpression boundExpression => Algebrizer.Algebrize(boundExpression),
                _ => throw ExceptionBuilder.UnexpectedValue(bindingResult.BoundRoot)
            };

            yield return LogicalShowPlanBuilder.Build(Resources.ShowPlanUnoptimized, logicalQuery);

            var outputColumns = logicalQuery.OutputColumns;
            var root = logicalQuery.Root;

            foreach (var (name, stepRoot) in LogicalOptimizer.GetOptimizationSteps(root, DataContext))
            {
                var stepName = string.Format(Resources.ShowPlanStepFmt, name);
                yield return LogicalShowPlanBuilder.Build(stepName, new LogicalQuery(stepRoot, outputColumns));
                root = stepRoot;
            }

            var optimizedQuery = new LogicalQuery(root, outputColumns);
            yield return LogicalShowPlanBuilder.Build(Resources.ShowPlanOptimized, optimizedQuery);

            var physicalQuery = Planner.Plan(optimizedQuery);
            yield return PhysicalShowPlanBuilder.Build(Resources.ShowPlanPhysical, physicalQuery);
        }
    }
}

#endif
