#nullable enable

using System.Collections.Frozen;
using System.Collections.Immutable;

using NQuery.Algebra;
using NQuery.AlgebraBinding;

namespace NQuery.Planning
{
    // Hash join over an equi-key (BuildKey == ProbeKey): the build input is hashed and
    // the probe input streamed against it. Remainder is the non-equi residual of the
    // join condition, evaluated on the combined build ++ probe row. Build is the join's
    // left and Probe its right, so the output (Build ++ Probe) keeps the join's own
    // (left ++ right) slot order.
    internal sealed class PhysicalHashMatch : PhysicalOperator
    {
        public PhysicalHashMatch(PhysicalHashMatchKind hashMatchKind, PhysicalOperator build, PhysicalOperator probe, ValueSlot buildKey, ValueSlot probeKey, ImmutableArray<LogicalExpression> remainder)
        {
            HashMatchKind = hashMatchKind;
            Build = build;
            Probe = probe;
            BuildKey = buildKey;
            ProbeKey = probeKey;
            Remainder = remainder;
        }

        public override PhysicalOperatorKind Kind => PhysicalOperatorKind.HashMatch;

        public PhysicalHashMatchKind HashMatchKind { get; }

        public PhysicalOperator Build { get; }

        public PhysicalOperator Probe { get; }

        public ValueSlot BuildKey { get; }

        public ValueSlot ProbeKey { get; }

        // The join condition's conjuncts other than the hash-key equality (may be empty).
        public ImmutableArray<LogicalExpression> Remainder { get; }

        protected override FrozenSet<ValueSlot> ComputeDefinedValueSlots() => Build.DefinedValueSlots.Concat(Probe.DefinedValueSlots).ToFrozenSet();

        protected override ImmutableArray<ValueSlot> ComputeOutputValueSlots() => Build.OutputValueSlots.Concat(Probe.OutputValueSlots).ToImmutableArray();
    }
}
