using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using NQuery.Binding;

namespace NQuery.Algebra
{
    internal sealed class AlgebraCombinedQuery : AlgebraRelation
    {
        private readonly AlgebraRelation _left;
        private readonly AlgebraRelation _right;
        private readonly AlgebraQueryCombinator _combinator;
        private readonly ReadOnlyCollection<ValueSlot> _outputValueSlots;

        public AlgebraCombinedQuery(AlgebraRelation left, AlgebraRelation right, AlgebraQueryCombinator combinator, IList<ValueSlot> outputValueSlots)
        {
            _left = left;
            _right = right;
            _combinator = combinator;
            _outputValueSlots = new ReadOnlyCollection<ValueSlot>(outputValueSlots);
        }

        public override AlgebraKind Kind
        {
            get { return AlgebraKind.BinaryQuery; }
        }

        public AlgebraRelation Left
        {
            get { return _left; }
        }

        public AlgebraRelation Right
        {
            get { return _right; }
        }

        public AlgebraQueryCombinator Combinator
        {
            get { return _combinator; }
        }

        public ReadOnlyCollection<ValueSlot> OutputValueSlots
        {
            get { return _outputValueSlots; }
        }
    }
}