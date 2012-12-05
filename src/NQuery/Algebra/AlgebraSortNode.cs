using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using NQuery.Binding;

namespace NQuery.Algebra
{
    internal class AlgebraSortNode : AlgebraRelation
    {
        private readonly AlgebraRelation _input;
        private readonly ReadOnlyCollection<ValueSlot> _valueSlots;

        public AlgebraSortNode(AlgebraRelation input, IList<ValueSlot> valueSlots)
        {
            _input = input;
            _valueSlots = new ReadOnlyCollection<ValueSlot>(valueSlots);
        }

        public override AlgebraKind Kind
        {
            get { return AlgebraKind.Sort; }
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