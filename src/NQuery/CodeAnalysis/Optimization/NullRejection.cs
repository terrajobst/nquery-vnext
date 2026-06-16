using NQuery.CodeAnalysis.Algebra;
using NQuery.CodeAnalysis.Binding;

namespace NQuery.CodeAnalysis.Optimization;

// Whether a predicate is guaranteed to evaluate to FALSE/UNKNOWN (i.e. reject the
// row) when the given value slot is NULL. Used by OuterJoinRemover to tighten an outer
// join into an inner one when a predicate above it rejects the null-supplied side.
//
// Conservative: only the node kinds below let us prove rejection; everything else
// (literals, variables, conversions, CASE, function calls) returns false, which only
// ever forgoes an optimization -- never an unsound one.
internal static class NullRejection
{
    public static bool IsRejectingNull(LogicalExpression expression, ValueSlot valueSlot)
    {
        switch (expression.Kind)
        {
            case LogicalExpressionKind.ValueSlot:
                return ((LogicalValueSlotExpression)expression).ValueSlot == valueSlot;

            case LogicalExpressionKind.IsNull:
                // `e IS NULL` is TRUE when e is NULL, so it *keeps* the row exactly when
                // its operand would reject it -- the rejection flips.
                return !IsRejectingNull(((LogicalIsNullExpression)expression).Expression, valueSlot);

            case LogicalExpressionKind.Unary:
                var unary = (LogicalUnaryExpression)expression;
                return unary.OperatorKind == UnaryOperatorKind.LogicalNot
                    ? !IsRejectingNull(unary.Expression, valueSlot)
                    : IsRejectingNull(unary.Expression, valueSlot);

            case LogicalExpressionKind.Binary:
                var binary = (LogicalBinaryExpression)expression;
                // OR rejects only if *both* sides reject (either surviving keeps the row);
                // every other operator yields NULL/FALSE if *either* operand is NULL.
                return binary.OperatorKind == BinaryOperatorKind.LogicalOr
                    ? IsRejectingNull(binary.Left, valueSlot) && IsRejectingNull(binary.Right, valueSlot)
                    : IsRejectingNull(binary.Left, valueSlot) || IsRejectingNull(binary.Right, valueSlot);

            case LogicalExpressionKind.PropertyAccess:
                return IsRejectingNull(((LogicalPropertyAccessExpression)expression).Target, valueSlot);

            case LogicalExpressionKind.MethodInvocation:
                // A NULL argument doesn't guarantee a NULL result, but a NULL target does.
                return IsRejectingNull(((LogicalMethodInvocationExpression)expression).Target, valueSlot);

            default:
                return false;
        }
    }
}
