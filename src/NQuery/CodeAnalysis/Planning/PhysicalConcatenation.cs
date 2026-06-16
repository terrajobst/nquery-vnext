using System.Collections.Frozen;
using System.Collections.Immutable;

using NQuery.CodeAnalysis.Algebra;

namespace NQuery.CodeAnalysis.Planning;

// Pure concatenation (UNION ALL): the inputs' rows in order. A plain UNION's
// distinctness is a separate distinct sort the planner places above this node, so
// this node carries no IsUnionAll flag of its own.
internal sealed class PhysicalConcatenation : PhysicalOperator
{
    public PhysicalConcatenation(ImmutableArray<PhysicalOperator> inputs, ImmutableArray<LogicalUnifiedValue> definedValues)
    {
        Inputs = inputs;
        DefinedValues = definedValues;
    }

    public override PhysicalOperatorKind Kind => PhysicalOperatorKind.Concatenation;

    public ImmutableArray<PhysicalOperator> Inputs { get; }

    public ImmutableArray<LogicalUnifiedValue> DefinedValues { get; }

    protected override FrozenSet<ValueSlot> ComputeDefinedValueSlots() => DefinedValues.Select(v => v.ValueSlot).ToFrozenSet();

    protected override ImmutableArray<ValueSlot> ComputeOutputValueSlots() => DefinedValues.Select(v => v.ValueSlot).ToImmutableArray();
}
