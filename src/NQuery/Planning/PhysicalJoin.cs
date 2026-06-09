#nullable enable

using System.Collections.Frozen;
using System.Collections.Immutable;

using NQuery.Algebra;
using NQuery.Binding;

namespace NQuery.Planning
{
    // The physical realization of a logical join. Algorithm is the planning choice:
    // a hash match for equi-joins, nested loops otherwise. Slot flow is identical to
    // the logical join.
    internal sealed class PhysicalJoin : PhysicalOperator
    {
        public PhysicalJoin(PhysicalJoinAlgorithm algorithm, LogicalJoinKind joinKind, PhysicalOperator left, PhysicalOperator right, ImmutableArray<LogicalExpression> conditions, ValueSlot? probe, LogicalExpression? passthruPredicate)
        {
            Algorithm = algorithm;
            JoinKind = joinKind;
            Left = left;
            Right = right;
            Conditions = conditions;
            Probe = probe;
            PassthruPredicate = passthruPredicate;
        }

        public override PhysicalOperatorKind Kind => PhysicalOperatorKind.Join;

        public PhysicalJoinAlgorithm Algorithm { get; }

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
