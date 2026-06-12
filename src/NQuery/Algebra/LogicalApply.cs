#nullable enable

using System.Collections.Frozen;
using System.Collections.Immutable;

using NQuery.AlgebraBinding;

namespace NQuery.Algebra
{
    // A dependent join: for each row of Left, the Right subtree is evaluated and
    // may reference Left's slots (that reference is the correlation). Decorrelation
    // pushes the Apply down until Right no longer depends on Left, turning it into
    // an ordinary join.
    //
    //   * Inner / LeftOuter expose Right's columns.
    //   * LeftSemi / LeftAntiSemi are "mark" applies: Right is consumed for an existence
    //     test and not exposed; Probe is a boolean slot recording whether a match
    //     was found.
    internal sealed class LogicalApply : LogicalOperator
    {
        public LogicalApply(LogicalApplyKind applyKind, LogicalOperator left, LogicalOperator right, ValueSlot? probe, LogicalExpression? passthru = null)
        {
            ApplyKind = applyKind;
            Left = left;
            Right = right;
            Probe = probe;
            Passthru = passthru;
        }

        public override LogicalOperatorKind Kind => LogicalOperatorKind.Apply;

        public LogicalApplyKind ApplyKind { get; }

        public LogicalOperator Left { get; }

        public LogicalOperator Right { get; }

        // The mark column for Semi/AntiSemi applies; null for Inner/LeftOuter.
        public ValueSlot? Probe { get; }

        // A left-row predicate (over Left's columns) under which the Right is *not*
        // evaluated -- the row passes straight through (null-padded / unmatched). It
        // restores CASE short-circuiting for a subquery inside a CASE branch: the guard
        // is the condition under which that branch isn't taken. Null for an ordinary
        // (unconditional) apply. A guarded apply is left as nested loops -- it is not
        // decorrelated, so the executor's passthru handling honors the guard.
        public LogicalExpression? Passthru { get; }

        // The left's output columns the right side references -- i.e. the correlation.
        // Lazily computed (and cached) by walking the right subtree once.
        public ImmutableArray<ValueSlot> OuterReferences
        {
            get
            {
                if (field.IsDefault)
                    ImmutableInterlocked.InterlockedInitialize(ref field, ComputeOuterReferences());
                return field;
            }
        }

        private bool ExposesRight => ApplyKind is LogicalApplyKind.Inner or LogicalApplyKind.LeftOuter;

        private ImmutableArray<ValueSlot> ComputeOuterReferences()
        {
            var referenced = LogicalSlotReferenceFinder.FindReferencedSlots(Right);
            return Left.OutputValueSlots.Where(referenced.Contains).ToImmutableArray();
        }

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
