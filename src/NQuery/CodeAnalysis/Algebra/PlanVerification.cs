using System.Collections.Frozen;
using System.Collections.Immutable;

namespace NQuery.CodeAnalysis.Algebra;

// Shared structural checks for the logical and physical plan verifiers (both
// DEBUG-only: the logical one runs after every optimizer pass that changed the tree,
// the physical one after planning). Two invariants live here:
//
//  * Slot availability (Require): each verifier walks its tree carrying the outer
//    scope an enclosing dependent join (apply) exposes, and asserts every referenced
//    slot is available -- produced by the operator's own input(s) or supplied by that
//    outer scope.
//  * Slot uniqueness (RequireUniqueDefinitions): no value slot is minted by two
//    operators. Availability proves every reference resolves to *some* definition;
//    uniqueness proves that definition is unambiguous -- together they pin slot
//    resolution down completely.
//
// Centralizing the checks and their messages keeps the two verifiers from drifting and
// keeps the diagnostics uniformly actionable.
internal static class PlanVerification
{
    public static void Require(string source, string operatorLabel, string role, IEnumerable<ValueSlot> referenced, FrozenSet<ValueSlot> available)
    {
        foreach (var slot in referenced)
        {
            if (!available.Contains(slot))
                throw Failure(source, operatorLabel, role, slot, available);
        }
    }

    public static void Require(string source, string operatorLabel, string role, LogicalExpression? expression, FrozenSet<ValueSlot> available)
    {
        if (expression is null)
            return;

        Require(source, operatorLabel, role, LogicalSlotReferenceFinder.FindReferencedSlots(expression), available);
    }

    // Asserts that no value slot is *introduced* (minted) by more than one operator in
    // the tree. A slot a node introduces is one it defines that none of its inputs
    // already did -- the rest of its DefinedValueSlots are merely threaded up from below
    // (definitions are cumulative), so subtracting the inputs isolates the genuinely new
    // slots. The walk is generic over the node type so the logical and physical trees
    // share it; each verifier supplies how to read a node's children, its defined slots,
    // and a label for diagnostics.
    public static void RequireUniqueDefinitions<TNode>(
        string source,
        TNode root,
        Func<TNode, ImmutableArray<TNode>> children,
        Func<TNode, FrozenSet<ValueSlot>> definedSlots,
        Func<TNode, string> label)
        where TNode : class
    {
        var introducedBy = new Dictionary<ValueSlot, string>();
        Walk(root);

        void Walk(TNode node)
        {
            var nodeChildren = children(node);
            foreach (var child in nodeChildren)
                Walk(child);

            var inherited = new HashSet<ValueSlot>();
            foreach (var child in nodeChildren)
                inherited.UnionWith(definedSlots(child));

            foreach (var slot in definedSlots(node))
            {
                if (inherited.Contains(slot))
                    continue;

                if (introducedBy.TryGetValue(slot, out var firstOperator))
                    throw DuplicateDefinition(source, slot, firstOperator, label(node));

                introducedBy[slot] = label(node);
            }
        }
    }

    private static InvalidOperationException DuplicateDefinition(string source, ValueSlot slot, string firstOperator, string secondOperator)
    {
        var message =
            $"{source}: value slot '{slot.Name}' is introduced by more than one operator -- both a {firstOperator} and a {secondOperator} mint it. " +
            $"Every value slot must be defined by exactly one operator; two definitions make slot resolution ambiguous and can silently read the wrong column. " +
            $"The usual cause is a rewrite that duplicated a subtree (or otherwise reused its minted slots) instead of giving the copy fresh ones.";

        return new InvalidOperationException(message);
    }

    private static InvalidOperationException Failure(string source, string operatorLabel, string role, ValueSlot slot, FrozenSet<ValueSlot> available)
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
