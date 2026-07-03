using System.Collections.Immutable;

using NQuery.CodeAnalysis.Algebra;
using NQuery.CodeAnalysis.Binding;

namespace NQuery.CodeAnalysis.Optimization;

// Prepares a surviving correlated apply for the planner's index spool. When a
// correlated equality's *outer* side is a computed expression over the apply's
// left (e.g. o.CustomerID = c.CustomerID + '!'), the planner can't spool it: the
// probe must be a plain outer slot, and there is nowhere below the spool to attach
// a compute that reads the outer row (see Planner.TryFindSpoolKey, which rejects a
// computed outer side).
//
// This pass moves that computation onto the left input -- a LogicalCompute
// materializing #probe = f(left), evaluated once per outer row -- and rewrites the
// conjunct to `inner = #probe`. The apply's OuterReferences are derived from what
// the right reads (LogicalApply.ComputeOuterReferences), so #probe becomes a plain
// outer slot automatically; the planner's existing plain-probe path then builds the
// spool with no planner change. The extra slot is masked by the enclosing
// projection, the same way the hash match's computed key is (PlanJoin).
//
// Doing it here rather than in the planner keeps the compute placement subject to
// the later passes (ColumnPruner keeps it live via the correlation; SelectionPushdown
// sees the rewritten predicate) and leaves the planner choosing implementations only.
// It runs after decorrelation has settled, so it only ever sees applies that survived
// -- a decorrelatable one becomes a join, where the hash match already materializes a
// computed key on either side itself.
internal sealed class SpoolProbeHoist : LogicalOperatorRewriter
{
    public static SpoolProbeHoist Instance { get; } = new();

    private SpoolProbeHoist()
    {
    }

    protected override LogicalOperator RewriteApply(LogicalApply node)
    {
        // Bottom-up: nested applies (and their own computed probes) are hoisted first,
        // each onto its own left.
        var rewritten = (LogicalApply)base.RewriteApply(node);

        var hoister = new Worker(rewritten.Left.OutputValueSlots);
        var right = hoister.RewriteRelation(rewritten.Right);
        if (hoister.Computed.Count == 0)
            return rewritten;

        var left = new LogicalCompute(rewritten.Left, [.. hoister.Computed]);
        return new LogicalApply(rewritten.ApplyKind, left, right, rewritten.Probe, rewritten.Passthru);
    }

    // Walks a single apply's right subtree, rewriting each correlated equality whose
    // outer side is computed over the left, and collecting the computes to place on
    // the left. Stops at a nested apply: that correlation belongs to the nested left
    // and is handled when the outer pass reaches it.
    private sealed class Worker : LogicalOperatorRewriter
    {
        private readonly ImmutableArray<ValueSlot> _leftSlots;

        public Worker(ImmutableArray<ValueSlot> leftSlots)
        {
            _leftSlots = leftSlots;
        }

        public List<LogicalComputedValue> Computed { get; } = [];

        protected override LogicalOperator RewriteApply(LogicalApply node) => node;

        protected override LogicalOperator RewriteFilter(LogicalFilter node)
        {
            var rewritten = (LogicalFilter)base.RewriteFilter(node);
            var inputSlots = rewritten.Input.OutputValueSlots;

            ImmutableArray<LogicalExpression>.Builder? conditions = null;
            for (var i = 0; i < rewritten.Conditions.Length; i++)
            {
                if (TryHoist(rewritten.Conditions[i], inputSlots, out var replacement))
                {
                    conditions ??= rewritten.Conditions.ToBuilder();
                    conditions[i] = replacement;
                }
            }

            return conditions is null
                ? rewritten
                : new LogicalFilter(rewritten.Input, conditions.MoveToImmutable());
        }

        // An equality with one side computed over the left alone (the probe) and the
        // other over the filter input alone (the index key). A plain outer side already
        // works in the planner, so only a computed one is hoisted.
        private bool TryHoist(LogicalExpression condition, ImmutableArray<ValueSlot> inputSlots, out LogicalExpression replacement)
        {
            replacement = null!;

            if (condition is not LogicalBinaryExpression { OperatorKind: BinaryOperatorKind.Equal } equal)
                return false;

            LogicalExpression outerSide;
            LogicalExpression innerSide;
            if (IsComputedOverLeft(equal.Left) && ReferencesOnly(equal.Right, inputSlots))
            {
                outerSide = equal.Left;
                innerSide = equal.Right;
            }
            else if (IsComputedOverLeft(equal.Right) && ReferencesOnly(equal.Left, inputSlots))
            {
                outerSide = equal.Right;
                innerSide = equal.Left;
            }
            else
            {
                return false;
            }

            var referenced = LogicalSlotReferenceFinder.FindReferencedSlots(outerSide);
            var probe = referenced.First().Factory.CreateTemporary(outerSide.Type);
            Computed.Add(new LogicalComputedValue(outerSide, probe));

            // Reuse the '=' overload: #probe carries the outer side's type, so the
            // resolved operator signature still applies. Preserve the original orientation.
            var probeExpression = new LogicalValueSlotExpression(probe);
            replacement = ReferenceEquals(outerSide, equal.Left)
                ? new LogicalBinaryExpression(probeExpression, equal.OperatorKind, equal.Result, innerSide)
                : new LogicalBinaryExpression(innerSide, equal.OperatorKind, equal.Result, probeExpression);
            return true;
        }

        private bool IsComputedOverLeft(LogicalExpression expression)
        {
            return expression is not LogicalValueSlotExpression && ReferencesOnly(expression, _leftSlots);
        }

        private static bool ReferencesOnly(LogicalExpression expression, ImmutableArray<ValueSlot> slots)
        {
            var referenced = LogicalSlotReferenceFinder.FindReferencedSlots(expression);
            return referenced.Count > 0 && referenced.All(slots.Contains);
        }
    }
}
