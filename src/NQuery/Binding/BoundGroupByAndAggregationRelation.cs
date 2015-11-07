using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace NQuery.Binding
{
    internal sealed class BoundGroupByAndAggregationRelation : BoundRelation
    {
        public BoundGroupByAndAggregationRelation(BoundRelation input, IEnumerable<ValueSlot> groups, IEnumerable<BoundAggregatedValue> aggregates)
        {
            Input = input;
            Groups = groups.ToImmutableArray();
            Aggregates = aggregates.ToImmutableArray();
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.GroupByAndAggregationRelation; }
        }

        public BoundRelation Input { get; }

        public ImmutableArray<ValueSlot> Groups { get; }

        public ImmutableArray<BoundAggregatedValue> Aggregates { get; }

        public BoundGroupByAndAggregationRelation Update(BoundRelation input, IEnumerable<ValueSlot> groups, IEnumerable<BoundAggregatedValue> aggregates)
        {
            var newGroups = groups.ToImmutableArray();
            var newAggregates = aggregates.ToImmutableArray();

            if (input == Input && newGroups == Groups && newAggregates == Aggregates)
                return this;

            return new BoundGroupByAndAggregationRelation(input, newGroups, newAggregates);
        }

        public override IEnumerable<ValueSlot> GetDefinedValues()
        {
            return GetOutputValues();
        }

        public override IEnumerable<ValueSlot> GetOutputValues()
        {
            return Groups.Concat(Aggregates.Select(v => v.Output));
        }
    }
}