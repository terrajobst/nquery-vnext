using System.Collections.Frozen;
using System.Collections.Immutable;

namespace NQuery.CodeAnalysis.Algebra;

// The self-reference inside a recursive CTE member: a leaf that scans the working
// table of the LogicalRecursiveUnion carrying the same token. It reads like a
// non-empty table scan -- it defines its own output slots, one per CTE column,
// positional to the union's DefinedValues -- so no pass mistakes it for something
// it may fold away or push into.
internal sealed class LogicalRecursiveReference : LogicalOperator
{
    private readonly ImmutableArray<ValueSlot> _outputValueSlots;

    public LogicalRecursiveReference(RecursionToken token, ImmutableArray<ValueSlot> outputValueSlots)
    {
        ThrowIfNull(token);

        Token = token;
        _outputValueSlots = outputValueSlots;
    }

    public override LogicalOperatorKind Kind => LogicalOperatorKind.RecursiveReference;

    public RecursionToken Token { get; }

    protected override FrozenSet<ValueSlot> ComputeDefinedValueSlots() => _outputValueSlots.ToFrozenSet();

    protected override ImmutableArray<ValueSlot> ComputeOutputValueSlots() => _outputValueSlots;
}
