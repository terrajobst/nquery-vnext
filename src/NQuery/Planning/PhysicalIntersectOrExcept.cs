#nullable enable

using System.Collections;
using System.Collections.Frozen;
using System.Collections.Immutable;

using NQuery.Binding;

namespace NQuery.Planning
{
    internal sealed class PhysicalIntersectOrExcept : PhysicalOperator
    {
        public PhysicalIntersectOrExcept(bool isIntersect, PhysicalOperator left, PhysicalOperator right, ImmutableArray<IComparer> comparers)
        {
            IsIntersect = isIntersect;
            Left = left;
            Right = right;
            Comparers = comparers;
        }

        public override PhysicalOperatorKind Kind => PhysicalOperatorKind.IntersectOrExcept;

        public bool IsIntersect { get; }

        public PhysicalOperator Left { get; }

        public PhysicalOperator Right { get; }

        public ImmutableArray<IComparer> Comparers { get; }

        protected override FrozenSet<ValueSlot> ComputeDefinedValueSlots() => Left.OutputValueSlots.ToFrozenSet();

        protected override ImmutableArray<ValueSlot> ComputeOutputValueSlots() => Left.OutputValueSlots;
    }
}
