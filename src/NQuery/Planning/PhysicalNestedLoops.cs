#nullable enable

using System.Collections.Frozen;
using System.Collections.Immutable;

using NQuery.Algebra;
using NQuery.Binding;

namespace NQuery.Planning
{
    // The nested-loops realization of a logical join: for each outer (left) row it
    // rescans the inner (right). It is the general-purpose join algorithm -- it works
    // for any predicate. Equi-joins will get a sibling PhysicalHashMatch once that is
    // built; until then every join is planned as nested loops. Slot flow is identical
    // to the logical join.
    //
    // It is also the physical form of an Apply (a dependent join): IsDependent means
    // the right may reference the left's columns (outer references), so the executor
    // threads the left row into the right's evaluation. A plain join has IsDependent
    // = false; an apply has IsDependent = true, no Conditions (the correlation lives
    // in the right's own filters), and its ApplyKind mapped onto JoinKind.
    internal sealed class PhysicalNestedLoops : PhysicalOperator
    {
        public PhysicalNestedLoops(LogicalJoinKind joinKind, PhysicalOperator left, PhysicalOperator right, ImmutableArray<LogicalExpression> conditions, ValueSlot? probe, LogicalExpression? passthruPredicate, bool isDependent)
        {
            JoinKind = joinKind;
            Left = left;
            Right = right;
            Conditions = conditions;
            Probe = probe;
            PassthruPredicate = passthruPredicate;
            IsDependent = isDependent;
        }

        public override PhysicalOperatorKind Kind => PhysicalOperatorKind.NestedLoops;

        public LogicalJoinKind JoinKind { get; }

        public PhysicalOperator Left { get; }

        public PhysicalOperator Right { get; }

        public ImmutableArray<LogicalExpression> Conditions { get; }

        public ValueSlot? Probe { get; }

        public LogicalExpression? PassthruPredicate { get; }

        // True for an Apply: the right side is re-evaluated per left row with the
        // left's columns in scope as outer references.
        public bool IsDependent { get; }

        protected override FrozenSet<ValueSlot> ComputeDefinedValueSlots()
        {
            var result = Left.DefinedValueSlots.Concat(Right.DefinedValueSlots);
            if (Probe is not null)
                result = result.Append(Probe);
            return result.ToFrozenSet();
        }

        protected override ImmutableArray<ValueSlot> ComputeOutputValueSlots()
        {
            IEnumerable<ValueSlot> result = Left.OutputValueSlots;
            if (IncludeRightValues())
                result = result.Concat(Right.OutputValueSlots);
            if (Probe is not null)
                result = result.Append(Probe);
            return result.ToImmutableArray();
        }

        private bool IncludeRightValues()
        {
            return JoinKind switch
            {
                LogicalJoinKind.Inner or LogicalJoinKind.LeftOuter or LogicalJoinKind.FullOuter => true,
                LogicalJoinKind.LeftSemi or LogicalJoinKind.LeftAntiSemi => false,
                _ => throw ExceptionBuilder.UnexpectedValue(JoinKind)
            };
        }
    }
}
