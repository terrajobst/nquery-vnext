using System;

namespace NQuery.Binding
{
    internal static class NullRejection
    {
        public static bool IsRejectingNull(BoundExpression expression, ValueSlot valueSlot)
        {
            // Only for node types listed below we can predict if the expression will yield
            // null/false. For all other node types this is unknown.

            switch (expression.Kind)
            {
                case BoundNodeKind.IsNullExpression:
                    return IsRejectingNull((BoundIsNullExpression)expression, valueSlot);
                case BoundNodeKind.UnaryExpression:
                    return IsRejectingNull((BoundUnaryExpression)expression, valueSlot);
                case BoundNodeKind.BinaryExpression:
                    return IsRejectingNull((BoundBinaryExpression)expression, valueSlot);
                case BoundNodeKind.ColumnExpression:
                    return IsRejectingNull((BoundColumnExpression)expression, valueSlot);
                case BoundNodeKind.ValueSlotExpression:
                    return IsRejectingNull((BoundValueSlotExpression)expression, valueSlot);
                case BoundNodeKind.PropertyAccessExpression:
                    return IsRejectingNull((BoundPropertyAccessExpression)expression, valueSlot);
                case BoundNodeKind.MethodInvocationExpression:
                    return IsRejectingNull((BoundMethodInvocationExpression)expression, valueSlot);
                default:
                    return false;
            }
        }

        private static bool IsRejectingNull(BoundIsNullExpression expression, ValueSlot valueSlot)
        {
            return !IsRejectingNull(expression.Expression, valueSlot);
        }

        private static bool IsRejectingNull(BoundUnaryExpression expression, ValueSlot valueSlot)
        {
            if (expression.OperatorKind == UnaryOperatorKind.LogicalNot)
                return !IsRejectingNull(expression.Expression, valueSlot);

            return IsRejectingNull(expression.Expression, valueSlot);
        }

        private static bool IsRejectingNull(BoundBinaryExpression expression, ValueSlot valueSlot)
        {
            if (expression.OperatorKind == BinaryOperatorKind.LogicalOr)
            {
                // Special handling for logical OR:
                // For logical OR both arguments must be NULL to yield FALSE/NULL.

                return IsRejectingNull(expression.Left, valueSlot) &&
                       IsRejectingNull(expression.Right, valueSlot);
            }

            // In all other cases we know the result will be FALSE/NULL if
            // any operand is NULL.

            return IsRejectingNull(expression.Left, valueSlot) ||
                   IsRejectingNull(expression.Right, valueSlot);
        }

        private static bool IsRejectingNull(BoundColumnExpression expression, ValueSlot valueSlot)
        {
            return expression.Symbol.ValueSlot == valueSlot;
        }

        private static bool IsRejectingNull(BoundValueSlotExpression expression, ValueSlot valueSlot)
        {
            return expression.ValueSlot == valueSlot;
        }

        private static bool IsRejectingNull(BoundPropertyAccessExpression expression, ValueSlot valueSlot)
        {
            return IsRejectingNull(expression.Target, valueSlot);
        }

        private static bool IsRejectingNull(BoundMethodInvocationExpression expression, ValueSlot valueSlot)
        {
            // NOTE: Arguments being NULL doesn't necessarily mean that the result will be NULL.

            return IsRejectingNull(expression.Target, valueSlot);
        }
    }
}
