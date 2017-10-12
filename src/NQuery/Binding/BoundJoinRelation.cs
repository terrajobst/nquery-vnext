using System;
using System.Collections.Generic;
using System.Linq;

namespace NQuery.Binding
{
    internal sealed class BoundJoinRelation : BoundRelation
    {
        public BoundJoinRelation(BoundJoinType joinType, BoundRelation left, BoundRelation right, BoundExpression condition, ValueSlot probe, BoundExpression passthruPredicate)
        {
            Left = left;
            Right = right;
            JoinType = joinType;
            Condition = condition;
            Probe = probe;
            PassthruPredicate = passthruPredicate;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.JoinRelation; }
        }

        public BoundRelation Left { get; }

        public BoundRelation Right { get; }

        public BoundJoinType JoinType { get; }

        public BoundExpression Condition { get; }

        public ValueSlot Probe { get; }

        public BoundExpression PassthruPredicate { get; }

        public BoundJoinRelation Update(BoundJoinType joinType, BoundRelation left, BoundRelation right, BoundExpression condition, ValueSlot probe, BoundExpression passthruPredicate)
        {
            if (joinType == JoinType && left == Left && right == Right && condition == Condition && probe == Probe && passthruPredicate == PassthruPredicate)
                return this;

            return new BoundJoinRelation(joinType, left, right, condition, probe, passthruPredicate);
        }

        public BoundJoinRelation WithJoinType(BoundJoinType joinType)
        {
            return Update(joinType, Left, Right, Condition, Probe, PassthruPredicate);
        }

        public BoundJoinRelation WithLeft(BoundRelation left)
        {
            return Update(JoinType, left, Right, Condition, Probe, PassthruPredicate);
        }

        public BoundJoinRelation WithRight(BoundRelation right)
        {
            return Update(JoinType, Left, right, Condition, Probe, PassthruPredicate);
        }

        public BoundJoinRelation WithCondition(BoundExpression condition)
        {
            return Update(JoinType, Left, Right, condition, Probe, PassthruPredicate);
        }

        public override IEnumerable<ValueSlot> GetDefinedValues()
        {
            var probe = Probe == null
                ? Enumerable.Empty<ValueSlot>()
                : new[] {Probe};
            return Left.GetDefinedValues().Concat(Right.GetDefinedValues()).Concat(probe);
        }

        public override IEnumerable<ValueSlot> GetOutputValues()
        {
            var left = IncludeLeftValues() ? Left.GetOutputValues() : Enumerable.Empty<ValueSlot>();
            var right = IncludeRightValues() ? Right.GetOutputValues() : Enumerable.Empty<ValueSlot>();
            var leftAndRight = left.Concat(right);
            return Probe == null
                    ? leftAndRight
                    : leftAndRight.Concat(new[] {Probe});
        }

        private bool IncludeLeftValues()
        {
            switch (JoinType)
            {
                case BoundJoinType.Inner:
                case BoundJoinType.FullOuter:
                case BoundJoinType.LeftOuter:
                case BoundJoinType.RightOuter:
                case BoundJoinType.LeftSemi:
                case BoundJoinType.LeftAntiSemi:
                    return true;
                default:
                    throw ExceptionBuilder.UnexpectedValue(JoinType);
            }
        }

        private bool IncludeRightValues()
        {
            switch (JoinType)
            {
                case BoundJoinType.Inner:
                case BoundJoinType.FullOuter:
                case BoundJoinType.LeftOuter:
                case BoundJoinType.RightOuter:
                    return true;
                case BoundJoinType.LeftSemi:
                case BoundJoinType.LeftAntiSemi:
                    return false;
                default:
                    throw ExceptionBuilder.UnexpectedValue(JoinType);
            }
        }
    }
}