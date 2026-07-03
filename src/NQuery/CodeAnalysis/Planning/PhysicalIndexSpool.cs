using System.Collections.Frozen;
using System.Collections.Immutable;

using NQuery.CodeAnalysis.Algebra;

namespace NQuery.CodeAnalysis.Planning;

// A lazy index spool: the physical form of a correlated equality filter whose input
// is outer-independent (see Planner.TryPlanIndexSpool). On first open the input is
// scanned once and indexed by IndexKey; every (re-)open then probes the index with
// the current value of ProbeKey -- an outer slot -- and emits only the matching
// rows. Turns a per-outer-row re-scan into one scan plus per-row lookups.
internal sealed class PhysicalIndexSpool : PhysicalOperator
{
    public PhysicalIndexSpool(PhysicalOperator input, ValueSlot indexKey, ValueSlot probeKey)
    {
        ThrowIfNull(input);
        ThrowIfNull(indexKey);
        ThrowIfNull(probeKey);

        Input = input;
        IndexKey = indexKey;
        ProbeKey = probeKey;
    }

    public override PhysicalOperatorKind Kind => PhysicalOperatorKind.IndexSpool;

    public PhysicalOperator Input { get; }

    // The input column the spool is indexed by.
    public ValueSlot IndexKey { get; }

    // The outer slot whose current value is looked up on every open.
    public ValueSlot ProbeKey { get; }

    protected override FrozenSet<ValueSlot> ComputeDefinedValueSlots() => Input.DefinedValueSlots;

    protected override ImmutableArray<ValueSlot> ComputeOutputValueSlots() => Input.OutputValueSlots;
}
