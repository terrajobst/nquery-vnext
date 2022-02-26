using System.Collections.Immutable;

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
                if (element == rewrittenElement && result == null)
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

        protected IEnumerable<BoundAggregatedValue> RewriteAggregatedValues(ImmutableArray<BoundAggregatedValue> list)
        {
            return RewriteArray(list, RewriteAggregatedValue);
        }

        protected IEnumerable<ValueSlot> RewriteValueSlots(ImmutableArray<ValueSlot> list)
        {
            return RewriteArray(list, RewriteValueSlot);
        }

        protected IEnumerable<BoundComputedValue> RewriteComputedValues(ImmutableArray<BoundComputedValue> list)
        {
            return RewriteArray(list, RewriteComputedValue);
        }

        protected IEnumerable<BoundUnifiedValue> RewriteUnifiedValues(ImmutableArray<BoundUnifiedValue> list)
        {
            return RewriteArray(list, RewriteUnifiedValue);
        }

        protected IEnumerable<BoundComparedValue> RewriteComparedValues(ImmutableArray<BoundComparedValue> list)
        {
            return RewriteArray(list, RewriteComparedValue);
        }

        protected IEnumerable<BoundExpression> RewriteExpressions(ImmutableArray<BoundExpression> list)
        {
            return RewriteArray(list, RewriteExpression);
        }

        protected IEnumerable<BoundCaseLabel> RewriteCaseLabels(ImmutableArray<BoundCaseLabel> list)
        {
            return RewriteArray(list, RewriteCaseLabel);
        }

        protected IEnumerable<BoundRelation> RewriteRelations(ImmutableArray<BoundRelation> list)
        {
            return RewriteArray(list, RewriteRelation);
        }

        protected virtual BoundAggregatedValue RewriteAggregatedValue(BoundAggregatedValue value)
        {
            return value.Update(RewriteValueSlot(value.Output),
                               value.Aggregate,
                               value.Aggregatable,
                               RewriteExpression(value.Argument));
        }

        protected virtual BoundComputedValue RewriteComputedValue(BoundComputedValue value)
        {
            return value.Update(RewriteExpression(value.Expression),
                               RewriteValueSlot(value.ValueSlot));
        }

        protected virtual BoundUnifiedValue RewriteUnifiedValue(BoundUnifiedValue value)
        {
            return value.Update(RewriteValueSlot(value.ValueSlot),
                               RewriteValueSlots(value.InputValueSlots));
        }

        protected virtual BoundComparedValue RewriteComparedValue(BoundComparedValue value)
        {
            return value.Update(RewriteValueSlot(value.ValueSlot),
                                value.Comparer);
        }

        protected virtual ValueSlot RewriteValueSlot(ValueSlot valueSlot)
        {
            return valueSlot;
        }

        protected virtual BoundCaseLabel RewriteCaseLabel(BoundCaseLabel node)
        {
            return node.Update(RewriteExpression(node.Condition),
                               RewriteExpression(node.ThenExpression));
        }
    }
}