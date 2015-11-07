using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace NQuery.Binding
{
    internal sealed class BoundTopRelation : BoundRelation
    {
        public BoundTopRelation(BoundRelation input, int limit, IEnumerable<BoundSortedValue> tieEntries)
        {
            Input = input;
            Limit = limit;
            TieEntries = tieEntries.ToImmutableArray();
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.TopRelation;  }
        }

        public BoundRelation Input { get; }

        public int Limit { get; }

        public ImmutableArray<BoundSortedValue> TieEntries { get; }

        public BoundTopRelation Update(BoundRelation input, int limit, IEnumerable<BoundSortedValue> tieEntries)
        {
            var newTieEntries = tieEntries.ToImmutableArray();

            if (input == Input && limit == Limit && newTieEntries == TieEntries)
                return this;

            return new BoundTopRelation(input, limit, newTieEntries);
        }

        public override IEnumerable<ValueSlot> GetDefinedValues()
        {
            return Input.GetDefinedValues();
        }

        public override IEnumerable<ValueSlot> GetOutputValues()
        {
            return Input.GetOutputValues();
        }
    }
}