using System;
using System.Collections.Generic;
using System.Linq;

namespace NQuery.Binding
{
    internal sealed class BoundIntersectOrExceptRelation : BoundRelation
    {
        private readonly bool _isIntersect;
        private readonly BoundRelation _left;
        private readonly BoundRelation _right;

        public BoundIntersectOrExceptRelation(bool isIntersect, BoundRelation left, BoundRelation right)
        {
            _isIntersect = isIntersect;
            _left = left;
            _right = right;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.IntersectOrExceptRelation; }
        }

        public bool IsIntersect
        {
            get { return _isIntersect; }
        }

        public BoundRelation Left
        {
            get { return _left; }
        }

        public BoundRelation Right
        {
            get { return _right; }
        }

        public override IEnumerable<ValueSlot> GetDefinedValues()
        {
            return Enumerable.Empty<ValueSlot>();
        }

        public override IEnumerable<ValueSlot> GetOutputValues()
        {
            return _left.GetOutputValues();
        }

        public BoundIntersectOrExceptRelation Update(bool isIntersect, BoundRelation left, BoundRelation right)
        {
            if (isIntersect == _isIntersect && left == _left && right == _right)
                return this;

            return new BoundIntersectOrExceptRelation(isIntersect, left, right);
        }
    }
}