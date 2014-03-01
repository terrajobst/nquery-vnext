using System;
using System.Collections.Generic;
using System.Linq;

namespace NQuery.Binding
{
    partial class BoundTreeRewriter
    {
        private static IList<T> RewriteList<T>(IList<T> list, Func<T, T> rewriter)
            where T: class
        {
            List<T> result = null;

            for (var i = 0; i < list.Count; i++)
            {
                var element = list[i];
                var rewrittenElement = rewriter(element);
                if (element == rewrittenElement)
                    continue;

                if (result == null)
                {
                    result = new List<T>(list.Count);
                    result.AddRange(list.Take(i));
                }

                if (rewrittenElement != null)
                    result.Add(rewrittenElement);
            }

            return result == null
                     ? list
                     : result;
        }

        private IList<BoundAggregatedValue> RewriteAggregatedValueList(IList<BoundAggregatedValue> list)
        {
            return RewriteList(list, RewriteAggregatedValue);
        }

        private IList<ValueSlot> RewriteValueSlotList(IList<ValueSlot> list)
        {
            return RewriteList(list, RewriteValueSlot);
        }

        private IList<BoundComputedValue> RewriteComputedValueList(IList<BoundComputedValue> list)
        {
            return RewriteList(list, RewriteComputedValue);
        }

        private IList<BoundSortedValue> RewriteSortedValueList(IList<BoundSortedValue> list)
        {
            return RewriteList(list, RewriteSortedValue);
        }

        private IList<BoundExpression> RewriteExpressionList(IList<BoundExpression> list)
        {
            return RewriteList(list, RewriteExpression);
        }

        private IList<BoundCaseLabel> RewriteCaseLabelList(IList<BoundCaseLabel> list)
        {
            return RewriteList(list, RewriteCaseLabel);
        }

        protected virtual BoundAggregatedValue RewriteAggregatedValue(BoundAggregatedValue node)
        {
            return node.Update(RewriteValueSlot(node.Output),
                               node.Aggregate,
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