using System;

namespace NQuery.Binding
{
    internal sealed class BoundJoinRelation : BoundRelation
    {
        private readonly BoundJoinType _joinType;
        private readonly BoundRelation _left;
        private readonly BoundRelation _right;
        private readonly BoundExpression _condition;

        public BoundJoinRelation(BoundJoinType joinType, BoundRelation left, BoundRelation right, BoundExpression condition)
        {
            _joinType = joinType;
            _left = left;
            _right = right;
            _condition = condition;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.JoinRelation; }
        }

        public BoundJoinType JoinType
        {
            get { return _joinType; }
        }

        public BoundRelation Left
        {
            get { return _left; }
        }

        public BoundRelation Right
        {
            get { return _right; }
        }

        public BoundExpression Condition
        {
            get { return _condition; }
        }
    }
}