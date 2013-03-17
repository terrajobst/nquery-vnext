using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using NQuery.Algebra;
using NQuery.Binding;
using NQuery.Symbols;

namespace NQuery.Plan
{
    internal sealed class ExpressionBuilder
    {
        private readonly static PropertyInfo RowBufferIndexer = typeof(RowBuffer).GetProperty("Item", new[] { typeof(int) });
        private readonly static PropertyInfo VariableSymbolValueProperty = typeof(VariableSymbol).GetProperty("Value", typeof(object));

        private readonly ValueSlotSettings _valueSlotSettings;

        private ExpressionBuilder(ValueSlotSettings valueSlotSettings)
        {
            _valueSlotSettings = valueSlotSettings;
        }

        public static Func<T> CompileExpression<T>(AlgebraExpression expression, ValueSlotSettings valueSlotSettings)
        {
            var builder = new ExpressionBuilder(valueSlotSettings);
            var targetType = typeof (T);
            var body = Expression.Convert(builder.BuildExpression(expression), targetType);
            var lambda = Expression.Lambda<Func<T>>(body);
            return lambda.Compile();
        }

        private static Expression ConvertToTargetType(Type type, Expression expression)
        {
            var targetType = GetTargetType(type);
            return expression.Type == targetType
                       ? expression
                       : Expression.Convert(expression, targetType);
        }

        private static Type GetTargetType(Type type)
        {
            // TODO: We should keep everything as nullable.
            //return type.IsValueType
            //           ? typeof (Nullable<>).MakeGenericType(type)
            //           : type;

            return type.IsNull() ? typeof(object) : type;
        }

        private Expression BuildExpression(AlgebraExpression expression)
        {
            switch (expression.Kind)
            {
                case AlgebraKind.UnaryExpression:
                    return BuildUnaryExpression((AlgebraUnaryExpression) expression);
                case AlgebraKind.BinaryExpression:
                    return BuildBinaryExpression((AlgebraBinaryExpression)expression);
                case AlgebraKind.LiteralExpression:
                    return BuildLiteralExpression((AlgebraLiteralExpression)expression);
                case AlgebraKind.ValueSlotExpression:
                    return BuildValueSlotExpression((AlgebraValueSlotExpression)expression);
                case AlgebraKind.VariableExpression:
                    return BuildVariableExpression((AlgebraVariableExpression)expression);
                case AlgebraKind.FunctionInvocationExpression:
                    return BuildFunctionInvocationExpression((AlgebraFunctionInvocationExpression)expression);
                case AlgebraKind.PropertyAccessExpression:
                    return BuildPropertyAccessExpression((AlgebraPropertyAccessExpression)expression);
                case AlgebraKind.MethodInvocationExpression:
                    return BuildMethodInvocationExpression((AlgebraMethodInvocationExpression)expression);
                case AlgebraKind.ConversionExpression:
                    return BuildConversionExpression((AlgebraConversionExpression)expression);
                case AlgebraKind.IsNullExpression:
                    return BuildIsNullExpression((AlgebraIsNullExpression)expression);
                case AlgebraKind.CaseExpression:
                    return BuildCaseExpression((AlgebraCaseExpression)expression);
                default:
                    throw new ArgumentOutOfRangeException("expression", string.Format("Unknown expression kind: {0}.", expression.Kind));
            }
        }

        private Expression BuildUnaryExpression(AlgebraUnaryExpression expression)
        {
            var input = BuildExpression(expression.Expression);
            var signature = expression.Signature;

            switch (signature.Kind)
            {
                case UnaryOperatorKind.Identity:
                    return Expression.UnaryPlus(input, signature.MethodInfo);
                case UnaryOperatorKind.Negation:
                    return Expression.Negate(input, signature.MethodInfo);
                case UnaryOperatorKind.Complement:
                    return Expression.OnesComplement(input, signature.MethodInfo);
                case UnaryOperatorKind.LogicalNot:
                    return Expression.Not(input, signature.MethodInfo);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private Expression BuildBinaryExpression(AlgebraBinaryExpression expression)
        {
            var left = BuildExpression(expression.Left);
            var right = BuildExpression(expression.Right);
            var signature = expression.Signature;

            switch (signature.Kind)
            {
                case BinaryOperatorKind.Multiply:
                    return Expression.Multiply(left, right, signature.MethodInfo);
                case BinaryOperatorKind.Divide:
                    return Expression.Divide(left, right, signature.MethodInfo);
                case BinaryOperatorKind.Modulus:
                    return Expression.Modulo(left, right, signature.MethodInfo);
                case BinaryOperatorKind.Add:
                    return Expression.Add(left, right, signature.MethodInfo);
                case BinaryOperatorKind.Sub:
                    return Expression.Subtract(left, right, signature.MethodInfo);
                case BinaryOperatorKind.Equal:
                    return Expression.Equal(left, right, false, signature.MethodInfo);
                case BinaryOperatorKind.NotEqual:
                    return Expression.NotEqual(left, right, false, signature.MethodInfo);
                case BinaryOperatorKind.Less:
                    return Expression.LessThan(left, right, false, signature.MethodInfo);
                case BinaryOperatorKind.LessOrEqual:
                    return Expression.LessThanOrEqual(left, right, false, signature.MethodInfo);
                case BinaryOperatorKind.Greater:
                    return Expression.GreaterThan(left, right, false, signature.MethodInfo);
                case BinaryOperatorKind.GreaterOrEqual:
                    return Expression.GreaterThanOrEqual(left, right, false, signature.MethodInfo);
                case BinaryOperatorKind.BitXor:
                    return Expression.ExclusiveOr(left, right, signature.MethodInfo);
                case BinaryOperatorKind.BitAnd:
                    return Expression.And(left, right, signature.MethodInfo);
                case BinaryOperatorKind.BitOr:
                    return Expression.Or(left, right, signature.MethodInfo);
                case BinaryOperatorKind.LeftShift:
                    return Expression.LeftShift(left, right, signature.MethodInfo);
                case BinaryOperatorKind.RightShift:
                    return Expression.RightShift(left, right, signature.MethodInfo);
                case BinaryOperatorKind.LogicalAnd:
                    return Expression.AndAlso(left, right, signature.MethodInfo);
                case BinaryOperatorKind.LogicalOr:
                    return Expression.OrElse(left, right, signature.MethodInfo);
                case BinaryOperatorKind.Power:
                case BinaryOperatorKind.Like:
                case BinaryOperatorKind.SimilarTo:
                case BinaryOperatorKind.Soundslike:
                    return Expression.Call(signature.MethodInfo, left, right);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private Expression BuildLiteralExpression(AlgebraLiteralExpression expression)
        {
            return Expression.Constant(expression.Value, GetTargetType(expression.Type));
        }

        private Expression BuildValueSlotExpression(AlgebraValueSlotExpression expression)
        {
            var rowBufferIndex = _valueSlotSettings.GetRowBufferIndex(expression.ValueSlot);
            var rowBufferFunc = _valueSlotSettings.RowBufferProvider;
            return
                ConvertToTargetType(
                    expression.ValueSlot.Type,
                    Expression.MakeIndex(
                        Expression.Invoke(
                            Expression.Constant(rowBufferFunc)
                        ),
                        RowBufferIndexer,
                        new[] { Expression.Constant(rowBufferIndex) }
                    )
                );
        }

        private Expression BuildVariableExpression(AlgebraVariableExpression expression)
        {
            return ConvertToTargetType(expression.Type, Expression.MakeMemberAccess(Expression.Constant(expression.Symbol), VariableSymbolValueProperty));
        }

        private Expression BuildFunctionInvocationExpression(AlgebraFunctionInvocationExpression expression)
        {
            var arguments = expression.Arguments.Select(BuildExpression);
            return expression.Symbol.CreateInvocation(arguments);
        }

        private Expression BuildPropertyAccessExpression(AlgebraPropertyAccessExpression expression)
        {
            var instance = BuildExpression(expression.Target);
            return expression.Symbol.CreateInvocation(instance);
        }

        private Expression BuildMethodInvocationExpression(AlgebraMethodInvocationExpression expression)
        {
            var instance = BuildExpression(expression.Target);
            var arguments = expression.Arguments.Select(BuildExpression);
            return expression.Symbol.CreateInvocation(instance, arguments);
        }

        private Expression BuildConversionExpression(AlgebraConversionExpression expression)
        {
            var input = BuildExpression(expression.Expression);
            var targetType = expression.Type;
            var conversionMethod = expression.Conversion.ConversionMethods.FirstOrDefault();
            return Expression.Convert(input, targetType, conversionMethod);
        }

        private Expression BuildIsNullExpression(AlgebraIsNullExpression expression)
        {
            var input = BuildExpression(expression.Expression);
            return Expression.ReferenceEqual(input, Expression.Constant(null, GetTargetType(TypeFacts.Null)));
        }

        private Expression BuildCaseExpression(AlgebraCaseExpression expression)
        {
            throw new NotImplementedException();
        }
    }
}