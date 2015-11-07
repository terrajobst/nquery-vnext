using System;
using System.Collections.Generic;
using System.Linq;

namespace NQuery.Binding
{
    internal sealed class BoundIntersectOrExceptRelation : BoundRelation
    {
        public BoundIntersectOrExceptRelation(bool isIntersect, BoundRelation left, BoundRelation right)
        {
            IsIntersect = isIntersect;
            Left = left;
            Right = right;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.IntersectOrExceptRelation; }
        }

        public bool IsIntersect { get; }

        public BoundRelation Left { get; }

        public BoundRelation Right { get; }

        public override IEnumerable<ValueSlot> GetDefinedValues()
        {
            return GetOutputValues();
        }

        public override IEnumerable<ValueSlot> GetOutputValues()
        {
            return Left.GetOutputValues();
        }

        public BoundIntersectOrExceptRelation Update(bool isIntersect, BoundRelation left, BoundRelation right)
        {
            if (isIntersect == IsIntersect && left == Left && right == Right)
                return this;

            return new BoundIntersectOrExceptRelation(isIntersect, left, right);
        }
    }
}