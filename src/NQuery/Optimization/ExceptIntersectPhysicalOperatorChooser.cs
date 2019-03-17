using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.Binding;

namespace NQuery.Optimization
{
    internal sealed class ExceptIntersectPhysicalOperatorChooser : BoundTreeRewriter
    {
        protected override BoundRelation RewriteIntersectOrExceptRelation(BoundIntersectOrExceptRelation node)
        {
            var left = RewriteRelation(node.Left);
            var right = RewriteRelation(node.Right);

            var values = left.GetOutputValues();
            var sortedValues = values.Zip(node.Comparers, (v, c) => new BoundComparedValue(v, c));
            var sortedLeft = new BoundSortRelation(true, left, sortedValues);

            var valueSlots = sortedLeft.GetOutputValues().Zip(right.GetOutputValues(), ValueTuple.Create);
            var condition = CreatePredicate(valueSlots);

            var joinOperator = node.IsIntersect
                ? BoundJoinType.LeftSemi
                : BoundJoinType.LeftAntiSemi;

            return new BoundJoinRelation(joinOperator, sortedLeft, right, condition, null, null);
        }

        private static BoundExpression CreatePredicate(IEnumerable<(ValueSlot, ValueSlot)> pairs)
        {
            BoundExpression result = null;

            foreach (var pair in pairs)
            {
                var left = pair.Item1;
                var right = pair.Item2;
                var predicate =
                    Expression.Or(
                        Expression.Equal(
                            Expression.Value(left),
                            Expression.Value(right)
                        ),
                        Expression.And(
                            Expression.IsNull(
                                Expression.Value(left)
                            ),
                            Expression.IsNull(
                                Expression.Value(right)
                            )
                        )
                    );

                result = Expression.And(result, predicate);
            }

            return result;
        }
    }
}