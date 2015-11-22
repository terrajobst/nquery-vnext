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
            var sortedValues = node.Groups.Select(g => new BoundComparedValue(g, Comparer.Default)).ToImmutableArray();

            if (node.Aggregates.IsEmpty)
                return new BoundSortRelation(true, input, sortedValues);

            var sortedInput = sortedValues.Any()
                ? new BoundSortRelation(false, input, sortedValues)
                : input;
            return new BoundStreamAggregatesRelation(sortedInput, node.Groups, node.Aggregates);
        }
    }
}