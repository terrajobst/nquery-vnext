using System;
using System.Collections.ObjectModel;

using NQuery.Binding;

namespace NQuery.Algebra
{
    internal sealed class AlgebraGroupByAndAggregation : AlgebraRelation
    {
        private readonly AlgebraRelation _input;
        private readonly ReadOnlyCollection<ValueSlot> _valueSlots;

        public AlgebraGroupByAndAggregation(AlgebraRelation input, ReadOnlyCollection<ValueSlot> valueSlots)
        {
            _input = input;
            _valueSlots = valueSlots;
        }

        public override AlgebraKind Kind
        {
            get { return AlgebraKind.GroupByAndAggregation; }
        }

        public AlgebraRelation Input
        {
            get { return _input; }
        }

        public ReadOnlyCollection<ValueSlot> ValueSlots
        {
            get { return _valueSlots; }
        }
    }
}