using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using NQuery.Binding;

namespace NQuery.Algebra
{
    internal sealed class AlgebraTopNode : AlgebraRelation
    {
        private readonly AlgebraRelation _input;
        private readonly int _limit;
        private readonly ReadOnlyCollection<ValueSlot> _tieEntries;
        private readonly ReadOnlyCollection<IComparer> _tieComparers;

        public AlgebraTopNode(AlgebraRelation input, int limit, IList<ValueSlot> tieEntries, IList<IComparer> tieComparer)
        {
            _input = input;
            _limit = limit;
            _tieEntries = new ReadOnlyCollection<ValueSlot>(tieEntries);
            _tieComparers = new ReadOnlyCollection<IComparer>(tieComparer);
        }

        public override AlgebraKind Kind
        {
            get { return AlgebraKind.Top; }
        }

        public AlgebraRelation Input
        {
            get { return _input; }
        }

        public int Limit
        {
            get { return _limit; }
        }

        public bool WithTies
        {
            get { return _tieEntries.Count > 0; }
        }

        public ReadOnlyCollection<ValueSlot> TieEntries
        {
            get { return _tieEntries; }
        }

        public ReadOnlyCollection<IComparer> TieComparers
        {
            get { return _tieComparers; }
        }
    }
}