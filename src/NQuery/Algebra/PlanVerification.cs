#nullable enable

namespace NQuery.Algebra;

// Shared slot-availability check for the logical and physical plan verifiers (both
// DEBUG-only: the logical one runs after every optimizer pass that changed the tree,
// the physical one after planning). Each verifier walks its tree carrying the outer
// scope an enclosing dependent join (apply) exposes, and asserts every referenced
// slot is available -- produced by the operator's own input(s) or supplied by that
// outer scope. Centralizing the check and its message keeps the two from drifting and
// keeps the diagnostic uniformly actionable.
internal static class PlanVerification
{
    public static void Require(string source, string operatorLabel, string role, IEnumerable<ValueSlot> referenced, ISet<ValueSlot> available)
    {
        foreach (var slot in referenced)
        {
            if (!available.Contains(slot))
                throw Failure(source, operatorLabel, role, slot, available);
        }
    }

    public static void Require(string source, string operatorLabel, string role, LogicalExpression? expression, ISet<ValueSlot> available)
    {
        if (expression is null)
            return;

        Require(source, operatorLabel, role, LogicalSlotReferenceFinder.FindReferencedSlots(expression), available);
    }

    private static InvalidOperationException Failure(string source, string operatorLabel, string role, ValueSlot slot, ISet<ValueSlot> available)
    {
        var inScope = available.Count == 0
            ? "(nothing)"
            : string.Join(", ", available.Select(s => s.Name).OrderBy(n => n, StringComparer.Ordinal));

        var message =
            $"{source}: the {operatorLabel} operator's {role} references value slot '{slot.Name}', which is out of scope here -- " +
            $"it is neither produced by this operator's input(s) nor supplied by an enclosing apply's correlation. " +
            $"In scope at this operator: {inScope}. " +
            $"This means the plan is malformed; the usual causes are a rewrite that dropped a slot still in use, " +
            $"one that moved an expression above the operator defining its slot, or one that failed to thread an " +
            $"apply's correlation into its right side.";

        return new InvalidOperationException(message);
    }
}
