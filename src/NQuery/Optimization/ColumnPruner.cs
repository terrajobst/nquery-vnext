using System.Collections.Immutable;

using NQuery.Algebra;

namespace NQuery.Optimization;

// Removes value slots that nothing references. The table scan defines a slot for every
// table column (Algebrizer.AlgebrizeNamedTableReference), so a query that touches
// only some columns still carries the rest through filters and -- crucially -- into
// the sort a stream aggregate inserts, where each spooled row is `new object[width]`.
// Narrowing the producers (table scan, compute, project, aggregate, union) cuts that
// per-row allocation.
//
// Liveness is top-down: a slot is live if a consumer above references it. We seed the
// set with the query's output slots, then walk pre-order -- recording the slots each
// operator consumes before descending -- so by the time we reach a producer every
// ancestor that needs one of its slots has been recorded. (The base rewriter is
// post-order, which is the wrong direction, so the traversal is hand-rolled in Worker.)
internal sealed class ColumnPruner : LogicalOperatorRewriter
{
    public static readonly ColumnPruner Instance = new();

    private ColumnPruner()
    {
    }

    public override LogicalOperator RewriteRelation(LogicalOperator node)
    {
        var worker = new Worker();
        worker.MarkUsed(node.OutputValueSlots);
        return worker.Rewrite(node);
    }

    private sealed class Worker
    {
        private readonly HashSet<ValueSlot> _used = new();

        public void MarkUsed(ValueSlot slot) => _used.Add(slot);

        public void MarkUsed(IEnumerable<ValueSlot> slots) => _used.UnionWith(slots);

        private void MarkExpression(LogicalExpression? expression)
        {
            if (expression is not null)
                _used.UnionWith(LogicalSlotReferenceFinder.FindReferencedSlots(expression));
        }

        public LogicalOperator Rewrite(LogicalOperator node)
        {
            switch (node.Kind)
            {
                case LogicalOperatorKind.Empty:
                case LogicalOperatorKind.Constant:
                    return node;
                case LogicalOperatorKind.TableScan:
                    return RewriteTableScan((LogicalTableScan)node);
                case LogicalOperatorKind.Filter:
                    return RewriteFilter((LogicalFilter)node);
                case LogicalOperatorKind.Compute:
                    return RewriteCompute((LogicalCompute)node);
                case LogicalOperatorKind.Project:
                    return RewriteProject((LogicalProject)node);
                case LogicalOperatorKind.Join:
                    return RewriteJoin((LogicalJoin)node);
                case LogicalOperatorKind.Apply:
                    return RewriteApply((LogicalApply)node);
                case LogicalOperatorKind.Aggregate:
                    return RewriteAggregate((LogicalAggregate)node);
                case LogicalOperatorKind.Union:
                    return RewriteUnion((LogicalUnion)node);
                case LogicalOperatorKind.IntersectOrExcept:
                    return RewriteIntersectOrExcept((LogicalIntersectOrExcept)node);
                case LogicalOperatorKind.Sort:
                    return RewriteSort((LogicalSort)node);
                case LogicalOperatorKind.Top:
                    return RewriteTop((LogicalTop)node);
                case LogicalOperatorKind.Assert:
                    return RewriteAssert((LogicalAssert)node);
                default:
                    throw ExceptionBuilder.UnexpectedValue(node.Kind);
            }
        }

        private LogicalOperator RewriteTableScan(LogicalTableScan node)
        {
            var slots = node.OutputValueSlots;

            var keep = new List<int>(slots.Length);
            for (var i = 0; i < slots.Length; i++)
            {
                if (_used.Contains(slots[i]))
                    keep.Add(i);
            }

            // Keep at least one column so the scan still iterates rows (e.g. for a bare
            // COUNT(*) that references no column); a zero-width scan isn't emittable.
            if (keep.Count == 0 && slots.Length > 0)
                keep.Add(0);

            if (keep.Count == slots.Length)
                return node;

            var definedValues = keep.Select(i => node.DefinedValues[i]).ToImmutableArray();
            var outputSlots = keep.Select(i => slots[i]).ToImmutableArray();
            return new LogicalTableScan(node.TableInstance, definedValues, outputSlots);
        }

        private LogicalOperator RewriteFilter(LogicalFilter node)
        {
            foreach (var condition in node.Conditions)
                MarkExpression(condition);

            var input = Rewrite(node.Input);
            return input == node.Input ? node : new LogicalFilter(input, node.Conditions);
        }

        private LogicalOperator RewriteCompute(LogicalCompute node)
        {
            var kept = node.DefinedValues.RemoveAll(v => !_used.Contains(v.ValueSlot));

            foreach (var value in kept)
                MarkExpression(value.Expression);

            var input = Rewrite(node.Input);

            // Nothing this compute defines is used anymore -- drop the operator entirely.
            if (kept.IsEmpty)
                return input;

            return input == node.Input && kept.Length == node.DefinedValues.Length
                ? node
                : new LogicalCompute(input, kept);
        }

        private LogicalOperator RewriteProject(LogicalProject node)
        {
            // A project whose outputs are all unused (e.g. the discarded right of a semi
            // join) prunes to zero columns -- that's a valid relation, the same shape the
            // Constant operator already emits (one row, no columns), so it still iterates.
            var kept = node.Outputs.RemoveAll(s => !_used.Contains(s));

            // A project's outputs are references to its input's slots, so whatever we keep
            // must stay live in the input below -- otherwise the input is pruned out from
            // under it.
            MarkUsed(kept);

            var input = Rewrite(node.Input);
            return input == node.Input && kept.Length == node.Outputs.Length
                ? node
                : new LogicalProject(input, kept);
        }

        private LogicalOperator RewriteJoin(LogicalJoin node)
        {
            foreach (var condition in node.Conditions)
                MarkExpression(condition);
            MarkExpression(node.PassthruPredicate);
            if (node.Probe is not null)
                MarkUsed(node.Probe);

            var left = Rewrite(node.Left);
            var right = Rewrite(node.Right);
            return left == node.Left && right == node.Right
                ? node
                : new LogicalJoin(node.JoinKind, left, right, node.Conditions, node.Probe, node.PassthruPredicate);
        }

        private LogicalOperator RewriteApply(LogicalApply node)
        {
            // The right subtree reads these left slots (the correlation); they must stay
            // live in the left no matter what the left itself exposes.
            MarkUsed(node.OuterReferences);
            MarkExpression(node.Passthru);
            if (node.Probe is not null)
                MarkUsed(node.Probe);

            // Right before left: visiting the right records every left slot it references,
            // so the left isn't pruned out from under it.
            var right = Rewrite(node.Right);
            var left = Rewrite(node.Left);
            return left == node.Left && right == node.Right
                ? node
                : new LogicalApply(node.ApplyKind, left, right, node.Probe, node.Passthru);
        }

        private LogicalOperator RewriteAggregate(LogicalAggregate node)
        {
            var keptAggregates = node.Aggregates.RemoveAll(a => !_used.Contains(a.Output));

            foreach (var group in node.Groups)
                MarkUsed(group.ValueSlot);
            foreach (var aggregate in keptAggregates)
                MarkExpression(aggregate.Argument);

            var input = Rewrite(node.Input);
            return input == node.Input && keptAggregates.Length == node.Aggregates.Length
                ? node
                : new LogicalAggregate(input, node.Groups, keptAggregates);
        }

        private LogicalOperator RewriteUnion(LogicalUnion node)
        {
            // Only UNION ALL maps to a plain concatenation whose columns can be narrowed;
            // a distinct UNION compares every column, so all stay live.
            var kept = node.IsUnionAll
                ? node.DefinedValues.RemoveAll(v => !_used.Contains(v.ValueSlot))
                : node.DefinedValues;

            foreach (var value in kept)
                MarkUsed(value.InputValueSlots);

            var inputs = RewriteMany(node.Inputs);
            return inputs == node.Inputs && kept.Length == node.DefinedValues.Length
                ? node
                : new LogicalUnion(node.IsUnionAll, inputs, kept, node.Comparers);
        }

        private LogicalOperator RewriteIntersectOrExcept(LogicalIntersectOrExcept node)
        {
            MarkUsed(node.Left.OutputValueSlots);
            MarkUsed(node.Right.OutputValueSlots);

            var left = Rewrite(node.Left);
            var right = Rewrite(node.Right);
            return left == node.Left && right == node.Right
                ? node
                : new LogicalIntersectOrExcept(node.IsIntersect, left, right, node.Comparers);
        }

        private LogicalOperator RewriteSort(LogicalSort node)
        {
            foreach (var sorted in node.SortedValues)
                MarkUsed(sorted.ValueSlot);

            var input = Rewrite(node.Input);
            return input == node.Input ? node : new LogicalSort(node.IsDistinct, input, node.SortedValues);
        }

        private LogicalOperator RewriteTop(LogicalTop node)
        {
            foreach (var tie in node.TieEntries)
                MarkUsed(tie.ValueSlot);

            var input = Rewrite(node.Input);
            return input == node.Input ? node : new LogicalTop(input, node.Limit, node.TieEntries);
        }

        private LogicalOperator RewriteAssert(LogicalAssert node)
        {
            MarkExpression(node.Condition);

            var input = Rewrite(node.Input);
            return input == node.Input ? node : new LogicalAssert(input, node.Condition, node.Message);
        }

        private ImmutableArray<LogicalOperator> RewriteMany(ImmutableArray<LogicalOperator> inputs)
        {
            ImmutableArray<LogicalOperator>.Builder? builder = null;

            for (var i = 0; i < inputs.Length; i++)
            {
                var rewritten = Rewrite(inputs[i]);
                if (rewritten != inputs[i] && builder is null)
                    builder = inputs.ToBuilder();
                if (builder is not null)
                    builder[i] = rewritten;
            }

            return builder?.MoveToImmutable() ?? inputs;
        }
    }
}
