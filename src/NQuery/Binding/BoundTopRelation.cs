using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NQuery.Binding
{
    internal sealed class BoundTopRelation : BoundRelation
    {
        private readonly BoundRelation _input;
        private readonly int _limit;
        private readonly ReadOnlyCollection<BoundSortedValue> _tieEntries;

        public BoundTopRelation(BoundRelation input, int limit, IList<BoundSortedValue> tieEntries)
        {
            _input = input;
            _limit = limit;
            _tieEntries = new ReadOnlyCollection<BoundSortedValue>(tieEntries);
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.TopRelation;  }
        }

        public BoundRelation Input
        {
            get { return _input; }
        }

        public int Limit
        {
            get { return _limit; }
        }

        public ReadOnlyCollection<BoundSortedValue> TieEntries
        {
            get { return _tieEntries; }
        }

        public BoundTopRelation Update(BoundRelation input, int limit, IList<BoundSortedValue> tieEntries)
        {
            if (input == _input && limit == _limit && tieEntries == _tieEntries)
                return this;

            return new BoundTopRelation(input, limit, tieEntries);
        }

        public override IEnumerable<ValueSlot> GetDefinedValues()
        {
            return Enumerable.Empty<ValueSlot>();
        }

        public override IEnumerable<ValueSlot> GetOutputValues()
        {
            return _input.GetOutputValues();
        }
    }
}