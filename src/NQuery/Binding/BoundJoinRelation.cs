using System;
using System.Collections.Generic;
using System.Linq;

namespace NQuery.Binding
{
    internal sealed class BoundJoinRelation : BoundRelation
    {
        private readonly BoundRelation _left;
        private readonly BoundRelation _right;
        private readonly BoundJoinType _joinType;
        private readonly BoundExpression _condition;
        private readonly ValueSlot _probe;
        private readonly BoundExpression _passthruPredicate;

        public BoundJoinRelation(BoundJoinType joinType, BoundRelation left, BoundRelation right, BoundExpression condition, ValueSlot probe, BoundExpression passthruPredicate)
        {
            _left = left;
            _right = right;
            _joinType = joinType;
            _condition = condition;
            _probe = probe;
            _passthruPredicate = passthruPredicate;
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

        public ValueSlot Probe
        {
            get { return _probe; }
        }

        public BoundExpression PassthruPredicate
        {
            get { return _passthruPredicate; }
        }

        public BoundJoinRelation Update(BoundJoinType joinType, BoundRelation left, BoundRelation right, BoundExpression condition, ValueSlot probe, BoundExpression passthruPredicate)
        {
            if (joinType == _joinType && left == _left && right == _right && condition == _condition && probe == _probe && passthruPredicate == _passthruPredicate)
                return this;

            return new BoundJoinRelation(joinType, left, right, condition, probe, passthruPredicate);
        }

        public override IEnumerable<ValueSlot> GetDefinedValues()
        {
            var probe = _probe == null
                ? Enumerable.Empty<ValueSlot>()
                : new[] {_probe};
            return _left.GetDefinedValues().Concat(_right.GetDefinedValues()).Concat(probe);
        }

        public override IEnumerable<ValueSlot> GetOutputValues()
        {
            var left = IncludeLeftValues() ? _left.GetOutputValues() : Enumerable.Empty<ValueSlot>();
            var right = IncludeRightValues() ? _right.GetOutputValues() : Enumerable.Empty<ValueSlot>();
            var leftAndRight = left.Concat(right);
            return _probe == null
                    ? leftAndRight
                    : leftAndRight.Concat(new[] {_probe});
        }

        private bool IncludeLeftValues()
        {
            switch (_joinType)
            {
                case BoundJoinType.Inner:
                case BoundJoinType.FullOuter:
                case BoundJoinType.LeftOuter:
                case BoundJoinType.RightOuter:
                case BoundJoinType.LeftSemi:
                case BoundJoinType.LeftAntiSemi:
                    return true;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool IncludeRightValues()
        {
            switch (_joinType)
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
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}