using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.Binding;

namespace NQuery.Optimization
{
    internal static class Condition
    {
        public static BoundExpression And(BoundExpression left, BoundExpression right)
        {
            if (left == null)
                return right;

            if (right == null)
                return left;

            var result = BinaryOperator.Resolve(BinaryOperatorKind.LogicalAnd, left.Type, right.Type);
            return new BoundBinaryExpression(left, result, right);
        }

        public static BoundExpression And(IEnumerable<BoundExpression> conditions)
        {
            return conditions.Aggregate<BoundExpression, BoundExpression>(null, (c, n) => c == null ? n : And(c, n));
        }

        public static IEnumerable<BoundExpression> SplitConjunctions(BoundExpression expression)
        {
            var stack = new Stack<BoundExpression>();
            stack.Push(expression);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                var binary = current as BoundBinaryExpression;
                if (binary != null && binary.Result.Selected.Signature.Kind == BinaryOperatorKind.LogicalAnd)
                {
                    stack.Push(binary.Left);
                    stack.Push(binary.Right);
                }
                else
                {
                    yield return binary;
                }
            }
        }

        public static bool DependsOnAny(this BoundExpression expression, IEnumerable<ValueSlot> valueSlots)
        {
            var finder = new ValueSlotDependencyFinder();
            finder.VisitExpression(expression);
            return finder.ValueSlots.Overlaps(valueSlots);
        }
    }
}