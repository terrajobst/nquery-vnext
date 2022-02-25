using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NQuery.Binding
{
    internal sealed class BoundTopRelation : BoundRelation
    {
        public BoundTopRelation(BoundRelation input, int limit, IEnumerable<BoundComparedValue> tieEntries)
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

        public ImmutableArray<BoundComparedValue> TieEntries { get; }

        public BoundTopRelation Update(BoundRelation input, int limit, IEnumerable<BoundComparedValue> tieEntries)
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