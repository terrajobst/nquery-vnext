#nullable enable

using System.Collections.Immutable;

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

        // The pipeline is a fixed, immutable list of batches. Rewriter passes hold
        // no instance state, so they are shared singletons (selection pushdown is
        // reused across two batches).
        private static readonly ImmutableArray<Batch> Batches =
        [
            // Decorrelate applies into joins and push selections down, to a fixed
            // point (both are oriented downward and converge).
            new("Decorrelation", BatchStrategy.FixedPoint, ApplyPushdown.Instance, SelectionPushdown.Instance),

            // Reorder inner-join regions and turn predicates into join conditions.
            new("Join ordering", BatchStrategy.Once, JoinOrderer.Instance),

            // Ordering places single-table conjuncts as join conditions; push those
            // down to the leaves.
            new("Selection", BatchStrategy.FixedPoint, SelectionPushdown.Instance),
        ];

        public static LogicalQuery Optimize(LogicalQuery query)
        {
            ArgumentNullException.ThrowIfNull(query);

            var root = Optimize(query.Root);
            return new LogicalQuery(root, query.OutputColumns);
        }

        public static LogicalOperator Optimize(LogicalOperator root)
        {
            ArgumentNullException.ThrowIfNull(root);

            foreach (var batch in Batches)
                root = RunBatch(batch, root);

            return root;
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
