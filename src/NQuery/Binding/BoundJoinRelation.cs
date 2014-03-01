using System;

namespace NQuery.Binding
{
    internal sealed class BoundJoinRelation : BoundRelation
    {
        private readonly BoundRelation _left;
        private readonly BoundRelation _right;
        private readonly BoundJoinType _joinType;
        private readonly BoundExpression _condition;

        public BoundJoinRelation(BoundJoinType joinType, BoundRelation left, BoundRelation right, BoundExpression condition)
        {
            _left = left;
            _right = right;
            _joinType = joinType;
            _condition = condition;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.JoinRelation; }
        }

        public BoundRelation Left
        {
            get { return _left; }
        }

        public BoundRelation Right
        {
            get { return _right; }
        }

        public BoundJoinType JoinType
        {
            get { return _joinType; }
        }

        public BoundExpression Condition
        {
            get { return _condition; }
        }

        public BoundJoinRelation Update(BoundJoinType joinType, BoundRelation left, BoundRelation right, BoundExpression condition)
        {
            if (joinType == _joinType && left == _left && right == _right && condition == _condition)
                return this;

            return new BoundJoinRelation(joinType, left, right, condition);
        }
    }
}