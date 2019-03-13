#nullable enable

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

            if (node.Aggregates.IsEmpty)
                return new BoundSortRelation(true, input, node.Groups);

            var sortedInput = node.Groups.Any()
                ? new BoundSortRelation(false, input, node.Groups)
                : input;
            return new BoundStreamAggregatesRelation(sortedInput, node.Groups, node.Aggregates);
        }
    }
}