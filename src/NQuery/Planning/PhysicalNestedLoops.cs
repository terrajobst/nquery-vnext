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
    internal sealed class PhysicalNestedLoops : PhysicalOperator
    {
        public PhysicalNestedLoops(LogicalJoinKind joinKind, PhysicalOperator left, PhysicalOperator right, ImmutableArray<LogicalExpression> conditions, ValueSlot? probe, LogicalExpression? passthruPredicate)
        {
            JoinKind = joinKind;
            Left = left;
            Right = right;
            Conditions = conditions;
            Probe = probe;
            PassthruPredicate = passthruPredicate;
        }

        public override PhysicalOperatorKind Kind => PhysicalOperatorKind.NestedLoops;

        public LogicalJoinKind JoinKind { get; }

        public PhysicalOperator Left { get; }

        public PhysicalOperator Right { get; }

        public ImmutableArray<LogicalExpression> Conditions { get; }

        public ValueSlot? Probe { get; }

        public LogicalExpression? PassthruPredicate { get; }

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
