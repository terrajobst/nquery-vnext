#nullable enable

using System.Collections.Frozen;
using System.Collections.Immutable;

using NQuery.Algebra;
using NQuery.Binding;

namespace NQuery.Planning
{
    // A correlated apply that decorrelation could not turn into a join: executed as
    // a dependent (correlated) nested-loops join.
    internal sealed class PhysicalApply : PhysicalOperator
    {
        public PhysicalApply(LogicalApplyKind applyKind, PhysicalOperator left, PhysicalOperator right, ValueSlot? probe)
        {
            ApplyKind = applyKind;
            Left = left;
            Right = right;
            Probe = probe;
        }

        public override PhysicalOperatorKind Kind => PhysicalOperatorKind.Apply;

        public LogicalApplyKind ApplyKind { get; }

        public PhysicalOperator Left { get; }

        public PhysicalOperator Right { get; }

        public ValueSlot? Probe { get; }

        private bool ExposesRight => ApplyKind is LogicalApplyKind.Inner or LogicalApplyKind.LeftOuter;

        protected override FrozenSet<ValueSlot> ComputeDefinedValueSlots()
        {
            IEnumerable<ValueSlot> result = Left.DefinedValueSlots;
            if (ExposesRight)
                result = result.Concat(Right.DefinedValueSlots);
            if (Probe is not null)
                result = result.Append(Probe);
            return result.ToFrozenSet();
        }

        protected override ImmutableArray<ValueSlot> ComputeOutputValueSlots()
        {
            IEnumerable<ValueSlot> result = Left.OutputValueSlots;
            if (ExposesRight)
                result = result.Concat(Right.OutputValueSlots);
            if (Probe is not null)
                result = result.Append(Probe);
            return result.ToImmutableArray();
        }
    }
}
