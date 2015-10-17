using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace NQuery.Binding
{
    internal sealed class BoundTopRelation : BoundRelation
    {
        private readonly BoundRelation _input;
        private readonly int _limit;
        private readonly ImmutableArray<BoundSortedValue> _tieEntries;

        public BoundTopRelation(BoundRelation input, int limit, IEnumerable<BoundSortedValue> tieEntries)
        {
            _input = input;
            _limit = limit;
            _tieEntries = tieEntries.ToImmutableArray();
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

        public ImmutableArray<BoundSortedValue> TieEntries
        {
            get { return _tieEntries; }
        }

        public BoundTopRelation Update(BoundRelation input, int limit, IEnumerable<BoundSortedValue> tieEntries)
        {
            var newTieEntries = tieEntries.ToImmutableArray();

            if (input == _input && limit == _limit && newTieEntries == _tieEntries)
                return this;

            return new BoundTopRelation(input, limit, newTieEntries);
        }

        public override IEnumerable<ValueSlot> GetDefinedValues()
        {
            return _input.GetDefinedValues();
        }

        public override IEnumerable<ValueSlot> GetOutputValues()
        {
            return _input.GetOutputValues();
        }
    }
}