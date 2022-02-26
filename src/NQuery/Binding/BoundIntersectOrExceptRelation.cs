using System.Collections;
using System.Collections.Immutable;

namespace NQuery.Binding
{
    internal sealed class BoundIntersectOrExceptRelation : BoundRelation
    {
        public BoundIntersectOrExceptRelation(bool isIntersect, BoundRelation left, BoundRelation right, IEnumerable<IComparer> comparers)
        {
            IsIntersect = isIntersect;
            Left = left;
            Right = right;
            Comparers = comparers.ToImmutableArray();
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.IntersectOrExceptRelation; }
        }

        public bool IsIntersect { get; }

        public BoundRelation Left { get; }

        public BoundRelation Right { get; }

        public ImmutableArray<IComparer> Comparers { get; }

        public override IEnumerable<ValueSlot> GetDefinedValues()
        {
            return GetOutputValues();
        }

        public override IEnumerable<ValueSlot> GetOutputValues()
        {
            return Left.GetOutputValues();
        }

        public BoundIntersectOrExceptRelation Update(bool isIntersect, BoundRelation left, BoundRelation right, IEnumerable<IComparer> comparers)
        {
            var newComparers = comparers.ToImmutableArray();

            if (isIntersect == IsIntersect && left == Left && right == Right && newComparers == Comparers)
                return this;

            return new BoundIntersectOrExceptRelation(isIntersect, left, right, newComparers);
        }
    }
}