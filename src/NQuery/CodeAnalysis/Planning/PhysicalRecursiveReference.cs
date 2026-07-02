using System.Collections.Frozen;
using System.Collections.Immutable;

using NQuery.CodeAnalysis.Algebra;

namespace NQuery.CodeAnalysis.Planning;

// The working-table scan inside a recursive member: a leaf that reads the current
// recursion frontier of the PhysicalRecursiveUnion carrying the same token. Its
// output slots are positional to the union's unified columns.
internal sealed class PhysicalRecursiveReference : PhysicalOperator
{
    private readonly ImmutableArray<ValueSlot> _outputValueSlots;

    public PhysicalRecursiveReference(RecursionToken token, ImmutableArray<ValueSlot> outputValueSlots)
    {
        Token = token;
        _outputValueSlots = outputValueSlots;
    }

    public override PhysicalOperatorKind Kind => PhysicalOperatorKind.RecursiveReference;

    public RecursionToken Token { get; }

    protected override FrozenSet<ValueSlot> ComputeDefinedValueSlots() => _outputValueSlots.ToFrozenSet();

    protected override ImmutableArray<ValueSlot> ComputeOutputValueSlots() => _outputValueSlots;
}
