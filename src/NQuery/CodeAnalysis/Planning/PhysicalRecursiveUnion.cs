using System.Collections.Frozen;
using System.Collections.Immutable;

using NQuery.CodeAnalysis.Algebra;

namespace NQuery.CodeAnalysis.Planning;

// The physical recursion driver (working-table model): the anchor's rows seed the
// working table, then the recursive members are re-executed per round against the
// previous round's rows -- which their PhysicalRecursiveReference leaves (same
// token) scan -- until a round produces nothing. Like the concatenation, the
// defined values describe which input slot feeds each unified output column;
// InputValueSlots[0] is the anchor's, the members follow in order.
internal sealed class PhysicalRecursiveUnion : PhysicalOperator
{
    public PhysicalRecursiveUnion(RecursionToken token, PhysicalOperator anchor, ImmutableArray<PhysicalOperator> recursiveMembers, ImmutableArray<LogicalUnifiedValue> definedValues)
    {
        Token = token;
        Anchor = anchor;
        RecursiveMembers = recursiveMembers;
        DefinedValues = definedValues;
    }

    public override PhysicalOperatorKind Kind => PhysicalOperatorKind.RecursiveUnion;

    public RecursionToken Token { get; }

    public PhysicalOperator Anchor { get; }

    public ImmutableArray<PhysicalOperator> RecursiveMembers { get; }

    public ImmutableArray<LogicalUnifiedValue> DefinedValues { get; }

    protected override FrozenSet<ValueSlot> ComputeDefinedValueSlots() => DefinedValues.Select(v => v.ValueSlot).ToFrozenSet();

    protected override ImmutableArray<ValueSlot> ComputeOutputValueSlots() => [.. DefinedValues.Select(v => v.ValueSlot)];
}
