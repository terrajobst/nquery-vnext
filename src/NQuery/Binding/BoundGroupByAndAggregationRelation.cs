using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace NQuery.Binding
{
    internal sealed class BoundGroupByAndAggregationRelation : BoundRelation
    {
        private readonly BoundRelation _input;
        private readonly ImmutableArray<ValueSlot> _groups;
        private readonly ImmutableArray<BoundAggregatedValue> _aggregates;

        public BoundGroupByAndAggregationRelation(BoundRelation input, IEnumerable<ValueSlot> groups, IEnumerable<BoundAggregatedValue> aggregates)
        {
            _input = input;
            _groups = groups.ToImmutableArray();
            _aggregates = aggregates.ToImmutableArray();
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.GroupByAndAggregationRelation; }
        }

        public BoundRelation Input
        {
            get { return _input; }
        }

        public ImmutableArray<ValueSlot> Groups
        {
            get { return _groups; }
        }

        public ImmutableArray<BoundAggregatedValue> Aggregates
        {
            get { return _aggregates; }
        }

        public BoundGroupByAndAggregationRelation Update(BoundRelation input, IEnumerable<ValueSlot> groups, IEnumerable<BoundAggregatedValue> aggregates)
        {
            var newGroups = groups.ToImmutableArray();
            var newAggregates = aggregates.ToImmutableArray();

            if (input == _input && newGroups == _groups && newAggregates == _aggregates)
                return this;

            return new BoundGroupByAndAggregationRelation(input, newGroups, newAggregates);
        }

        public override IEnumerable<ValueSlot> GetDefinedValues()
        {
            return _aggregates.Select(v => v.Output);
        }

        public override IEnumerable<ValueSlot> GetOutputValues()
        {
            return _input.GetOutputValues().Concat(GetDefinedValues());
        }
    }
}