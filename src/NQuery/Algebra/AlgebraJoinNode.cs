using System;

using NQuery.BoundNodes;

namespace NQuery.Algebra
{
    internal sealed class AlgebraJoinNode : AlgebraNode
    {
        private readonly AlgebraJoinKind _joinKind;
        private readonly AlgebraNode _left;
        private readonly AlgebraNode _right;
        private readonly ValueSlot _probeValue;
        private readonly BoundExpression _condition;

        public AlgebraJoinNode(AlgebraJoinKind joinKind, AlgebraNode left, AlgebraNode right, ValueSlot probeValue, BoundExpression condition)
        {
            _joinKind = joinKind;
            _left = left;
            _right = right;
            _probeValue = probeValue;
            _condition = condition;
        }

        public AlgebraJoinKind JoinKind
        {
            get { return _joinKind; }
        }

        public AlgebraNode Left
        {
            get { return _left; }
        }

        public AlgebraNode Right
        {
            get { return _right; }
        }

        public ValueSlot ProbeValue
        {
            get { return _probeValue; }
        }

        public BoundExpression Condition
        {
            get { return _condition; }
        }

        public override AlgebraKind Kind
        {
            get { return AlgebraKind.Join; }
        }
    }
}