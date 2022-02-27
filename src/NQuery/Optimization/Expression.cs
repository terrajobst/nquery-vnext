using NQuery.Binding;

namespace NQuery.Optimization
{
    internal static class Expression
    {
        public static BoundExpression Literal(object value)
        {
            return new BoundLiteralExpression(value);
        }

        public static BoundExpression Value(ValueSlot valueSlot)
        {
            return new BoundValueSlotExpression(valueSlot);
        }

        public static BoundExpression Null()
        {
            return Literal(null);
        }

        public static BoundExpression IsNull(BoundExpression expression)
        {
            return new BoundIsNullExpression(expression);
        }

        public static BoundExpression Not(BoundExpression expression)
        {
            const UnaryOperatorKind logicalNot = UnaryOperatorKind.LogicalNot;
            var result = UnaryOperator.Resolve(logicalNot, expression.Type);
            return new BoundUnaryExpression(logicalNot, result, expression);
        }

        public static BoundExpression Plus(BoundExpression left, BoundExpression right)
        {
            return Merge(left, right, BinaryOperatorKind.Add);
        }

        public static BoundExpression Equal(BoundExpression left, BoundExpression right)
        {
            return Merge(left, right, BinaryOperatorKind.Equal);
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
            if (left is null)
                return right;

            if (right is null)
                return left;

            var result = BinaryOperator.Resolve(operatorKind, left.Type, right.Type);
            return new BoundBinaryExpression(left, operatorKind, result, right);
        }

        private static BoundExpression Merge(IEnumerable<BoundExpression> conditions, BinaryOperatorKind operatorKind)
        {
            return conditions.Aggregate<BoundExpression, BoundExpression>(null, (c, n) => c is null ? n : Merge(c, n, operatorKind));
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
            if (expression is null)
                yield break;

            var stack = new Stack<BoundExpression>();
            stack.Push(expression);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                var binary = current as BoundBinaryExpression;
                if (binary is not null && binary.OperatorKind == operatorKind)
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
            return binary is not null && binary.OperatorKind == operatorKind;
        }

        public static bool DependsOnAny(this BoundExpression condition, IEnumerable<ValueSlot> valueSlots)
        {
            var finder = new ValueSlotDependencyFinder();
            finder.VisitExpression(condition);
            return finder.ValueSlots.Overlaps(valueSlots);
        }
    }
}