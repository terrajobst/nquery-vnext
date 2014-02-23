using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NQuery.Binding
{
    internal sealed class BoundGroupByAndAggregationRelation : BoundRelation
    {
        private readonly BoundRelation _input;
        private readonly IReadOnlyCollection<ValueSlot> _groups;
        private readonly IReadOnlyCollection<BoundAggregateDefinition> _aggregates;

        public BoundGroupByAndAggregationRelation(BoundRelation input, IList<ValueSlot> groups, IList<BoundAggregateDefinition> aggregates)
        {
            _input = input;
            _groups = new ReadOnlyCollection<ValueSlot>(groups);
            _aggregates = new ReadOnlyCollection<BoundAggregateDefinition>(aggregates);
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.GroupByAndAggregationRelation; }
        }
    }
}