using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.Binding;

namespace NQuery.Optimization
{
    internal static class Condition
    {
        public static BoundExpression Not(BoundExpression expression)
        {
            const UnaryOperatorKind logicalNot = UnaryOperatorKind.LogicalNot;
            var result = UnaryOperator.Resolve(logicalNot, expression.Type);
            return new BoundUnaryExpression(logicalNot, result, expression);
        }

        public static BoundExpression LessThan(BoundExpression left, BoundExpression right)
        {
            return Merge(left, right, BinaryOperatorKind.LessOrEqual);
        }

        public static BoundExpression And(BoundExpression left, BoundExpression right)
        {
            return Merge(left, right, BinaryOperatorKind.LogicalAnd);
        }

        public static BoundExpression And(IEnumerable<BoundExpression> conditions)
        {
            return Merge(conditions, BinaryOperatorKind.LogicalAnd);
        }

        public static BoundExpression Or(BoundExpression left, BoundExpression right)
        {
            return Merge(left, right, BinaryOperatorKind.LogicalOr);
        }

        public static BoundExpression Or(IEnumerable<BoundExpression> conditions)
        {
            return Merge(conditions, BinaryOperatorKind.LogicalOr);
        }

        private static BoundExpression Merge(BoundExpression left, BoundExpression right, BinaryOperatorKind operatorKind)
        {
            if (left == null)
                return right;

            if (right == null)
                return left;

            var result = BinaryOperator.Resolve(operatorKind, left.Type, right.Type);
            return new BoundBinaryExpression(left, operatorKind, result, right);
        }

        private static BoundExpression Merge(IEnumerable<BoundExpression> conditions, BinaryOperatorKind operatorKind)
        {
            return conditions.Aggregate<BoundExpression, BoundExpression>(null, (c, n) => c == null ? n : Merge(c, n, operatorKind));
        }

        public static IEnumerable<BoundExpression> SplitConjunctions(BoundExpression expression)
        {
            return Split(expression, BinaryOperatorKind.LogicalAnd);
        }

        public static IEnumerable<BoundExpression> SplitDisjunctions(BoundExpression expression)
        {
            return Split(expression, BinaryOperatorKind.LogicalOr);
        }

        private static IEnumerable<BoundExpression> Split(BoundExpression expression, BinaryOperatorKind operatorKind)
        {
            if (expression == null)
                yield break;

            var stack = new Stack<BoundExpression>();
            stack.Push(expression);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                var binary = current as BoundBinaryExpression;
                if (binary != null && binary.OperatorKind == operatorKind)
                {
                    stack.Push(binary.Left);
                    stack.Push(binary.Right);
                }
                else
                {
                    yield return current;
                }
            }
        }

        public static bool IsConjunction(BoundExpression condition)
        {
            return IsBinaryOperator(condition, BinaryOperatorKind.LogicalAnd);
        }

        public static bool IsDisjunction(BoundExpression condition)
        {
            return IsBinaryOperator(condition, BinaryOperatorKind.LogicalOr);
        }

        private static bool IsBinaryOperator(BoundExpression condition, BinaryOperatorKind operatorKind)
        {
            var binary = condition as BoundBinaryExpression;
            return binary != null && binary.OperatorKind == operatorKind;
        }

        public static bool DependsOnAny(this BoundExpression condition, IEnumerable<ValueSlot> valueSlots)
        {
            var finder = new ValueSlotDependencyFinder();
            finder.VisitExpression(condition);
            return finder.ValueSlots.Overlaps(valueSlots);
        }
    }
}