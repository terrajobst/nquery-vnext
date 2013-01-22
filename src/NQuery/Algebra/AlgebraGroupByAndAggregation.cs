using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using NQuery.Binding;

namespace NQuery.Algebra
{
    internal sealed class AlgebraGroupByAndAggregation : AlgebraRelation
    {
        private readonly AlgebraRelation _input;
        private readonly ReadOnlyCollection<ValueSlot> _groups;
        private readonly ReadOnlyCollection<AlgebraAggregateDefinition> _aggregates;

        public AlgebraGroupByAndAggregation(AlgebraRelation input, ReadOnlyCollection<ValueSlot> groups, IList<AlgebraAggregateDefinition> aggregates)
        {
            _input = input;
            _groups = groups;
            _aggregates = new ReadOnlyCollection<AlgebraAggregateDefinition>(aggregates);
        }

        public override AlgebraKind Kind
        {
            get { return AlgebraKind.GroupByAndAggregation; }
        }

        public AlgebraRelation Input
        {
            get { return _input; }
        }

        public ReadOnlyCollection<ValueSlot> Groups
        {
            get { return _groups; }
        }

        public ReadOnlyCollection<AlgebraAggregateDefinition> Aggregates
        {
            get { return _aggregates; }
        }
    }
}