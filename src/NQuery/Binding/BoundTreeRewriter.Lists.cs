using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace NQuery.Binding
{
    partial class BoundTreeRewriter
    {
        private static IEnumerable<T> RewriteArray<T>(ImmutableArray<T> array, Func<T, T> rewriter)
            where T: class
        {
            List<T> result = null;

            for (var i = 0; i < array.Length; i++)
            {
                var element = array[i];
                var rewrittenElement = rewriter(element);
                if (element == rewrittenElement)
                    continue;

                if (result == null)
                {
                    result = new List<T>(array.Length);
                    result.AddRange(array.Take(i));
                }

                if (rewrittenElement != null)
                    result.Add(rewrittenElement);
            }

            return (IEnumerable<T>) result ?? array;
        }

        private IEnumerable<BoundAggregatedValue> RewriteAggregatedValues(ImmutableArray<BoundAggregatedValue> list)
        {
            return RewriteArray(list, RewriteAggregatedValue);
        }

        private IEnumerable<ValueSlot> RewriteValueSlots(ImmutableArray<ValueSlot> list)
        {
            return RewriteArray(list, RewriteValueSlot);
        }

        private IEnumerable<BoundComputedValue> RewriteComputedValues(ImmutableArray<BoundComputedValue> list)
        {
            return RewriteArray(list, RewriteComputedValue);
        }

        private IEnumerable<BoundSortedValue> RewriteSortedValues(ImmutableArray<BoundSortedValue> list)
        {
            return RewriteArray(list, RewriteSortedValue);
        }

        private IEnumerable<BoundExpression> RewriteExpressions(ImmutableArray<BoundExpression> list)
        {
            return RewriteArray(list, RewriteExpression);
        }

        private IEnumerable<BoundCaseLabel> RewriteCaseLabels(ImmutableArray<BoundCaseLabel> list)
        {
            return RewriteArray(list, RewriteCaseLabel);
        }

        protected virtual BoundAggregatedValue RewriteAggregatedValue(BoundAggregatedValue node)
        {
            return node.Update(RewriteValueSlot(node.Output),
                               node.Aggregate,
                               node.Aggregatable,
                               RewriteExpression(node.Argument));
        }

        protected virtual BoundComputedValue RewriteComputedValue(BoundComputedValue node)
        {
            return node.Update(RewriteExpression(node.Expression),
                               RewriteValueSlot(node.ValueSlot));
        }

        protected virtual ValueSlot RewriteValueSlot(ValueSlot node)
        {
            return node;
        }

        protected virtual BoundSortedValue RewriteSortedValue(BoundSortedValue node)
        {
            return node.Update(RewriteValueSlot(node.ValueSlot),
                               node.Comparer);
        }

        protected virtual BoundCaseLabel RewriteCaseLabel(BoundCaseLabel node)
        {
            return node.Update(RewriteExpression(node.Condition),
                               RewriteExpression(node.ThenExpression));
        }
    }
}