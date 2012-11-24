using System;

namespace NQuery.Algebra
{
    internal sealed class AlgebraJoinNode : AlgebraRelation
    {
        private readonly AlgebraJoinKind _joinKind;
        private readonly AlgebraRelation _left;
        private readonly AlgebraRelation _right;
        private readonly AlgebraExpression _condition;

        public AlgebraJoinNode(AlgebraJoinKind joinKind, AlgebraRelation left, AlgebraRelation right, AlgebraExpression condition)
        {
            _joinKind = joinKind;
            _left = left;
            _right = right;
            _condition = condition;
        }

        public override AlgebraKind Kind
        {
            get { return AlgebraKind.Join; }
        }

        public AlgebraJoinKind JoinKind
        {
            get { return _joinKind; }
        }

        public AlgebraRelation Left
        {
            get { return _left; }
        }

        public AlgebraRelation Right
        {
            get { return _right; }
        }

        public AlgebraExpression Condition
        {
            get { return _condition; }
        }
    }
}