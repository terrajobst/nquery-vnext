using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace NQuery.Binding
{
    internal sealed class BoundStreamAggregatesRelation : BoundRelation
    {
        public BoundStreamAggregatesRelation(BoundRelation input, IEnumerable<BoundComparedValue> groups, IEnumerable<BoundAggregatedValue> aggregates)
        {
            Input = input;
            Groups = groups.ToImmutableArray();
            Aggregates = aggregates.ToImmutableArray();
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.StreamAggregatesRelation; }
        }

        public BoundRelation Input { get; }

        public ImmutableArray<BoundComparedValue> Groups { get; }

        public ImmutableArray<BoundAggregatedValue> Aggregates { get; }

        public BoundStreamAggregatesRelation Update(BoundRelation input, IEnumerable<BoundComparedValue> groups, IEnumerable<BoundAggregatedValue> aggregates)
        {
            var newGroups = groups.ToImmutableArray();
            var newAggregates = aggregates.ToImmutableArray();

            if (input == Input && newGroups == Groups && newAggregates == Aggregates)
                return this;

            return new BoundStreamAggregatesRelation(input, newGroups, newAggregates);
        }

        public override IEnumerable<ValueSlot> GetDefinedValues()
        {
            return GetOutputValues();
        }

        public override IEnumerable<ValueSlot> GetOutputValues()
        {
            return Groups.Select(g => g.ValueSlot).Concat(Aggregates.Select(v => v.Output));
        }
    }
}