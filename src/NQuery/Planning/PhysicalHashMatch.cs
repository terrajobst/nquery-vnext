#nullable enable

using System.Collections.Frozen;
using System.Collections.Immutable;

using NQuery.Algebra;
using NQuery.Binding;

namespace NQuery.Planning
{
    // Hash join over an equi-key (BuildKey == ProbeKey): the build input is hashed and
    // the probe input streamed against it. Remainder is the non-equi residual of the
    // join condition, evaluated on the combined build ++ probe row. Build is the join's
    // left and Probe its right, so an inner/outer match's output (Build ++ Probe) keeps
    // the join's own (left ++ right) slot order.
    //
    // A semi/anti hash match outputs the build (left) side only. When ProbeColumn is set
    // (a "probing" semi join, the decorrelated form of EXISTS), every build row is output
    // with a trailing boolean reporting whether it matched -- so the enclosing filter can
    // test it for EXISTS / NOT EXISTS.
    internal sealed class PhysicalHashMatch : PhysicalOperator
    {
        public PhysicalHashMatch(PhysicalHashMatchKind hashMatchKind, PhysicalOperator build, PhysicalOperator probe, ValueSlot buildKey, ValueSlot probeKey, ImmutableArray<LogicalExpression> remainder, ValueSlot? probeColumn = null)
        {
            HashMatchKind = hashMatchKind;
            Build = build;
            Probe = probe;
            BuildKey = buildKey;
            ProbeKey = probeKey;
            Remainder = remainder;
            ProbeColumn = probeColumn;
        }

        public override PhysicalOperatorKind Kind => PhysicalOperatorKind.HashMatch;

        public PhysicalHashMatchKind HashMatchKind { get; }

        public PhysicalOperator Build { get; }

        public PhysicalOperator Probe { get; }

        public ValueSlot BuildKey { get; }

        public ValueSlot ProbeKey { get; }

        // The join condition's conjuncts other than the hash-key equality (may be empty).
        public ImmutableArray<LogicalExpression> Remainder { get; }

        // The boolean match-marker slot of a probing semi join (null otherwise). A probing
        // semi join emits every build row, this slot carrying whether it had a match.
        public ValueSlot? ProbeColumn { get; }

        private bool IsSemiOrAnti => HashMatchKind is PhysicalHashMatchKind.LeftSemi or PhysicalHashMatchKind.LeftAntiSemi;

        protected override FrozenSet<ValueSlot> ComputeDefinedValueSlots()
        {
            var result = Build.DefinedValueSlots.Concat(Probe.DefinedValueSlots);
            if (ProbeColumn is not null)
                result = result.Append(ProbeColumn);
            return result.ToFrozenSet();
        }

        protected override ImmutableArray<ValueSlot> ComputeOutputValueSlots()
        {
            // Semi/anti output the build side only (the probe is consumed just to decide
            // the match); a probing semi join appends its match-marker column.
            if (IsSemiOrAnti)
            {
                return ProbeColumn is null
                    ? Build.OutputValueSlots
                    : Build.OutputValueSlots.Add(ProbeColumn);
            }

            return Build.OutputValueSlots.Concat(Probe.OutputValueSlots).ToImmutableArray();
        }
    }
}
