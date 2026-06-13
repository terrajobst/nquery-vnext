#nullable enable

using System.Collections;
using System.Collections.Immutable;

using NQuery;
using NQuery.Refactor.Algebra;

namespace NQuery.Refactor.Optimization
{
    // Driver for the logical optimization pipeline. Passes are grouped into ordered
    // batches; each batch runs either Once or to a FixedPoint.
    //
    // Batching is what lets passes with different needs coexist: the algebraic
    // rewrites (decorrelation, selection pushdown) are oriented toward a normal form
    // and converge, so they run to a fixed point; join ordering rebuilds regions
    // unconditionally (not idempotent by reference) and must run Once. Selection
    // pushdown runs again after ordering, because placing predicates as join
    // conditions exposes new single-table conjuncts to push to the leaves.
    //
    // The rewriter's identity short-circuit makes "did anything change?" a reference
    // comparison on the root; the iteration cap turns a non-idempotent rule in a
    // fixed-point batch into a loud failure instead of a hang.
    internal static class LogicalOptimizer
    {
        private const int MaxIterations = 100;

        // The batch list is built per call: ApplyPushdown needs the DataContext's comparers
        // (for the aggregate-decorrelation domain), so unlike the stateless singletons it is
        // constructed fresh each time.
        private static ImmutableArray<Batch> BuildBatches(DataContext dataContext)
        {
            var applyPushdown = new ApplyPushdown(type => ResolveComparer(dataContext, type));

            // OuterJoinRemover accumulates per-tree state, so (like ApplyPushdown) it is a
            // fresh instance rather than a shared singleton.
            var outerJoinRemover = new OuterJoinRemover();

            return
            [
                // Decorrelate applies into joins and push selections down, to a fixed
                // point (both are oriented downward and converge).
                new("Decorrelation", BatchStrategy.FixedPoint, applyPushdown, SelectionPushdown.Instance),

                // Tighten outer joins into inner ones where a predicate above rejects the
                // null-supplied side. Runs before join ordering so a freed inner join can
                // join its region and accept pushed-down selections.
                new("Outer join removal", BatchStrategy.Once, outerJoinRemover),

                // Reorder inner-join regions and turn predicates into join conditions.
                new("Join ordering", BatchStrategy.Once, JoinOrderer.Instance),

                // Ordering places single-table conjuncts as join conditions; push those
                // down to the leaves.
                new("Selection", BatchStrategy.FixedPoint, SelectionPushdown.Instance),

                // Drop value slots nothing references (e.g. unread table columns), so the
                // narrowest possible rows flow into sorts/spools. Runs last, after pushdown
                // has settled which slots each predicate truly needs.
                new("Column pruning", BatchStrategy.Once, ColumnPruner.Instance),
            ];
        }

        public static LogicalQuery Optimize(LogicalQuery query, DataContext dataContext)
        {
            ArgumentNullException.ThrowIfNull(query);
            ArgumentNullException.ThrowIfNull(dataContext);

            var root = Optimize(query.Root, dataContext);
            return new LogicalQuery(root, query.OutputColumns);
        }

        public static LogicalOperator Optimize(LogicalOperator root, DataContext dataContext)
        {
            ArgumentNullException.ThrowIfNull(root);
            ArgumentNullException.ThrowIfNull(dataContext);

            foreach (var batch in BuildBatches(dataContext))
                root = RunBatch(batch, root);

            return root;
        }

        // The engine's comparer for a type: a DataContext-registered comparer (walking up the
        // base-type chain), else Comparer.Default for a comparable type. Mirrors
        // GlobalBinder.LookupComparer so the domain grouping matches the binder's semantics.
        private static IComparer? ResolveComparer(DataContext dataContext, Type type)
        {
            for (var key = type; key is not null; key = key.BaseType)
            {
                if (dataContext.Comparers.TryGetValue(key, out var comparer))
                    return comparer;
            }

            return type.IsComparable() ? Comparer.Default : null;
        }

        // The per-pass replay used by ShowPlan: like Optimize, but it yields the tree (and
        // the pass that produced it) after each pass that actually changed it. It mirrors
        // RunBatch's batching but, unlike compilation, does not assert convergence -- a
        // non-idempotent pass would simply produce the per-iteration steps up to the cap.
        public static IEnumerable<(string Name, LogicalOperator Root)> GetOptimizationSteps(LogicalOperator root, DataContext dataContext)
        {
            ArgumentNullException.ThrowIfNull(root);
            ArgumentNullException.ThrowIfNull(dataContext);

            foreach (var batch in BuildBatches(dataContext))
            {
                var iterations = batch.Strategy == BatchStrategy.Once ? 1 : MaxIterations;

                for (var iteration = 0; iteration < iterations; iteration++)
                {
                    var before = root;

                    foreach (var pass in batch.Passes)
                    {
                        var rewritten = pass.RewriteRelation(root);
                        if (!ReferenceEquals(rewritten, root))
                            yield return (pass.GetType().Name, rewritten);

                        root = rewritten;
                    }

                    if (ReferenceEquals(root, before))
                        break;
                }
            }
        }

        private static LogicalOperator RunBatch(Batch batch, LogicalOperator root)
        {
            if (batch.Strategy == BatchStrategy.Once)
            {
                foreach (var pass in batch.Passes)
                    root = pass.RewriteRelation(root);

                return root;
            }

            for (var iteration = 0; iteration < MaxIterations; iteration++)
            {
                var before = root;

                foreach (var pass in batch.Passes)
                    root = pass.RewriteRelation(root);

                if (ReferenceEquals(root, before))
                    return root;
            }

            throw new InvalidOperationException($"Logical optimization batch '{batch.Name}' did not converge within {MaxIterations} iterations; a pass is likely not idempotent.");
        }

        private enum BatchStrategy
        {
            Once,
            FixedPoint
        }

        private sealed class Batch
        {
            public Batch(string name, BatchStrategy strategy, params LogicalOperatorRewriter[] passes)
            {
                Name = name;
                Strategy = strategy;
                Passes = [..passes];
            }

            public string Name { get; }

            public BatchStrategy Strategy { get; }

            public ImmutableArray<LogicalOperatorRewriter> Passes { get; }
        }
    }
}
