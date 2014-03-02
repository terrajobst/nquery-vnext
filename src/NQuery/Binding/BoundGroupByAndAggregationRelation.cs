using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NQuery.Binding
{
    internal sealed class BoundGroupByAndAggregationRelation : BoundRelation
    {
        private readonly BoundRelation _input;
        private readonly ReadOnlyCollection<ValueSlot> _groups;
        private readonly ReadOnlyCollection<BoundAggregatedValue> _aggregates;

        public BoundGroupByAndAggregationRelation(BoundRelation input, IList<ValueSlot> groups, IList<BoundAggregatedValue> aggregates)
        {
            _input = input;
            _groups = new ReadOnlyCollection<ValueSlot>(groups);
            _aggregates = new ReadOnlyCollection<BoundAggregatedValue>(aggregates);
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.GroupByAndAggregationRelation; }
        }

        public BoundRelation Input
        {
            get { return _input; }
        }

        public ReadOnlyCollection<ValueSlot> Groups
        {
            get { return _groups; }
        }

        public ReadOnlyCollection<BoundAggregatedValue> Aggregates
        {
            get { return _aggregates; }
        }

        public BoundGroupByAndAggregationRelation Update(BoundRelation input, IList<ValueSlot> groups, IList<BoundAggregatedValue> aggregates)
        {
            if (input == _input && groups == _groups && aggregates == _aggregates)
                return this;

            return new BoundGroupByAndAggregationRelation(input, groups, aggregates);
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