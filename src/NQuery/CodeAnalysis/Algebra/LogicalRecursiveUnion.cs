using System.Collections.Frozen;
using System.Collections.Immutable;

namespace NQuery.CodeAnalysis.Algebra;

// A recursive CTE as a single, opaque operator: the anchor subtree seeds a working
// table, then the recursive-member subtrees are re-evaluated against the previous
// round's rows (each member reads them through a LogicalRecursiveReference leaf
// carrying the same token) until a round produces nothing. Like LogicalUnion, the
// unified outputs are defined here: DefinedValues[i].InputValueSlots holds the
// anchor's slot first, then one slot per recursive member, in member order.
//
// Keeping the recursion a single node is what makes it an optimizer barrier for
// free: passes recurse *into* the anchor and member subtrees and optimize them
// normally, but nothing moves *across* the boundary and the reference is never
// decorrelated or folded away.
internal sealed class LogicalRecursiveUnion : LogicalOperator
{
    public LogicalRecursiveUnion(RecursionToken token, LogicalOperator anchor, ImmutableArray<LogicalOperator> recursiveMembers, ImmutableArray<LogicalUnifiedValue> definedValues)
    {
        Token = token;
        Anchor = anchor;
        RecursiveMembers = recursiveMembers;
        DefinedValues = definedValues;
    }

    public override LogicalOperatorKind Kind => LogicalOperatorKind.RecursiveUnion;

    public RecursionToken Token { get; }

    public LogicalOperator Anchor { get; }

    public ImmutableArray<LogicalOperator> RecursiveMembers { get; }

    // The unified output columns. Their order is the working table's column order --
    // the layout every reference leaf reads positionally.
    public ImmutableArray<LogicalUnifiedValue> DefinedValues { get; }

    protected override FrozenSet<ValueSlot> ComputeDefinedValueSlots() => DefinedValues.Select(v => v.ValueSlot).ToFrozenSet();

    protected override ImmutableArray<ValueSlot> ComputeOutputValueSlots() => [.. DefinedValues.Select(v => v.ValueSlot)];
}
