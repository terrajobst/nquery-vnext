using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using NQuery.Binding;

namespace NQuery.Algebra
{
    internal class AlgebraSortNode : AlgebraRelation
    {
        private readonly AlgebraRelation _input;
        private readonly ReadOnlyCollection<ValueSlot> _valueSlots;
        private readonly ReadOnlyCollection<IComparer> _comparers;

        public AlgebraSortNode(AlgebraRelation input, IList<ValueSlot> valueSlots, IList<IComparer> comparers)
        {
            _input = input;
            _valueSlots = new ReadOnlyCollection<ValueSlot>(valueSlots);
            _comparers = new ReadOnlyCollection<IComparer>(comparers);
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

        public ReadOnlyCollection<IComparer> Comparers
        {
            get { return _comparers; }
        }
    }
}