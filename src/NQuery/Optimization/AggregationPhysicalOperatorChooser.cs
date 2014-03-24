using System;
using System.Collections;
using System.Collections.Immutable;
using System.Linq;

using NQuery.Binding;

namespace NQuery.Optimization
{
    internal sealed class AggregationPhysicalOperatorChooser : BoundTreeRewriter
    {
        protected override BoundRelation RewriteGroupByAndAggregationRelation(BoundGroupByAndAggregationRelation node)
        {
            var input = RewriteRelation(node.Input);
            var sortedValues = node.Groups.Select(g => new BoundSortedValue(g, Comparer.Default)).ToImmutableArray();
            var sortedInput = new BoundSortRelation(input, sortedValues);
            return new BoundStreamAggregatesRelation(sortedInput, node.Groups, node.Aggregates);
        }
    }
}