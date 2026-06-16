using System.Collections;
using System.Collections.Immutable;

using NQuery.CodeAnalysis.Algebra;

namespace NQuery.CodeAnalysis.Optimization;

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

    // The batch list is built per call: ApplyPushdown needs the Catalog's comparers
    // (for the aggregate-decorrelation domain), so unlike the stateless singletons it is
    // constructed fresh each time.
    private static ImmutableArray<Batch> BuildBatches(Catalog catalog)
    {
        var applyPushdown = new ApplyPushdown(type => ResolveComparer(catalog, type));

        // OuterJoinRemover accumulates per-tree state, so (like ApplyPushdown) it is a
        // fresh instance rather than a shared singleton.
        var outerJoinRemover = new OuterJoinRemover();

        return
        [
            // Decorrelate applies into joins and push selections down, to a fixed
            // point (both are oriented downward and converge). ProjectMerger rides
            // along to collapse the redundant projection layers decorrelation leaves
            // behind; it only ever removes projects, so it keeps the batch convergent.
            new("Decorrelation", BatchStrategy.FixedPoint, applyPushdown, SelectionPushdown.Instance, ProjectMerger.Instance),

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

    public static LogicalQuery Optimize(LogicalQuery query, Catalog catalog)
    {
        ThrowIfNull(query);
        ThrowIfNull(catalog);

        var root = Optimize(query.Root, catalog);
        return new LogicalQuery(root, query.OutputColumns);
    }

    public static LogicalOperator Optimize(LogicalOperator root, Catalog catalog)
    {
        ThrowIfNull(root);
        ThrowIfNull(catalog);

#if DEBUG
        // Verify the algebrizer's output before any pass runs, so a malformed starting
        // point is attributed to algebrization rather than the first pass.
        LogicalPlanVerifier.Verify(root, "Logical plan verification failed (algebrized plan, before optimization)");
#endif

        foreach (var batch in BuildBatches(catalog))
            root = RunBatch(batch, root);

        return root;
    }

    // The engine's comparer for a type: a Catalog-registered comparer (walking up the
    // base-type chain), else Comparer.Default for a comparable type. Mirrors
    // GlobalBinder.LookupComparer so the domain grouping matches the binder's semantics.
    private static IComparer? ResolveComparer(Catalog catalog, Type type)
    {
        for (var key = type; key is not null; key = key.BaseType)
        {
            if (catalog.Comparers.TryGetValue(key, out var comparer))
                return comparer;
        }

        return type.IsComparable() ? Comparer.Default : null;
    }

    // The per-pass replay used by ShowPlan: like Optimize, but it yields the tree (and
    // the pass that produced it) after each pass that actually changed it. It mirrors
    // RunBatch's batching but, unlike compilation, does not assert convergence -- a
    // non-idempotent pass would simply produce the per-iteration steps up to the cap.
    public static IEnumerable<(string Name, LogicalOperator Root)> GetOptimizationSteps(LogicalOperator root, Catalog catalog)
    {
        ThrowIfNull(root);
        ThrowIfNull(catalog);

        foreach (var batch in BuildBatches(catalog))
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
                root = RunPass(pass, root);

            return root;
        }

        for (var iteration = 0; iteration < MaxIterations; iteration++)
        {
            var before = root;

            foreach (var pass in batch.Passes)
                root = RunPass(pass, root);

            if (ReferenceEquals(root, before))
                return root;
        }

        throw new InvalidOperationException($"Logical optimization batch '{batch.Name}' did not converge within {MaxIterations} iterations; a pass is likely not idempotent.");
    }

    private static LogicalOperator RunPass(LogicalOperatorRewriter pass, LogicalOperator root)
    {
        var rewritten = pass.RewriteRelation(root);

#if DEBUG
        // Verify only when the pass actually changed the tree (an unchanged tree was
        // already verified by whatever produced it), and name the pass so a malformed
        // result points straight at the culprit.
        if (!ReferenceEquals(rewritten, root))
            LogicalPlanVerifier.Verify(rewritten, $"Logical plan verification failed (after the '{pass.GetType().Name}' pass)");
#endif

        return rewritten;
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
            Passes = [.. passes];
        }

        public string Name { get; }

        public BatchStrategy Strategy { get; }

        public ImmutableArray<LogicalOperatorRewriter> Passes { get; }
    }
}
