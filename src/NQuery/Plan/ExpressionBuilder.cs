using System;
using System.Collections.Generic;
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
        private readonly List<ParameterExpression> _locals = new List<ParameterExpression>();
        private readonly List<Expression> _assignments = new List<Expression>();

        private ExpressionBuilder(ValueSlotSettings valueSlotSettings)
        {
            _valueSlotSettings = valueSlotSettings;
        }

        public static Func<T> BuildExpression<T>(AlgebraExpression expression, ValueSlotSettings valueSlotSettings)
        {
            var builder = new ExpressionBuilder(valueSlotSettings);
            return builder.BuildExpression<T>(expression);
        }

        private ParameterExpression BuildCachedExpression(AlgebraExpression expression)
        {
            var result = BuildExpression(expression);
            var liftedExpression = BuildLiftedExpression(result);
            var local = Expression.Variable(liftedExpression.Type);
            var assignment = Expression.Assign(local, liftedExpression);
            _locals.Add(local);
            _assignments.Add(assignment);
            return local;
        }

        private static Expression BuildLiftedExpression(Expression result)
        {
            return result.Type.CanBeNull()
                       ? result
                       : Expression.Convert(result, result.Type.GetNullableType());
        }

        private Expression BuildLiftedExpression(AlgebraExpression expression)
        {
            return BuildLiftedExpression(BuildExpression(expression));
        }

        private static Expression BuildLoweredExpression(Expression expression)
        {
            if (!expression.Type.IsNullableOfT())
                return expression;

            var nonNullableType = expression.Type.GetNonNullableType();
            return Expression.Convert(expression, nonNullableType);
        }

        private static Expression BuildNullValue(Type type)
        {
            return Expression.Constant(null, type.GetNullableType());
        }

        private static Expression BuildNullCheck(Expression expression)
        {
            return expression.Type.IsNullableOfT()
                       ? (Expression)Expression.Not(Expression.Property(expression, "HasValue"))
                       : Expression.ReferenceEqual(expression, Expression.Constant(null, expression.Type));
        }

        private static Expression BuildNullCheck(IEnumerable<Expression> expressions)
        {
            return expressions
                .Select(BuildNullCheck)
                .Aggregate<Expression, Expression>(null, (current, nullCheck) => current == null ? nullCheck : Expression.OrElse(current, nullCheck));
        }

        private static UnaryExpression BuildNullableTrue()
        {
            return Expression.Convert(Expression.Constant(true), typeof(bool?));
        }

        private Func<T> BuildExpression<T>(AlgebraExpression expression)
        {
            var targetType = typeof (T);
            var actualExpression = BuildCachedExpression(expression);
            var coalescedExpression = targetType.CanBeNull()
                                          ? (Expression) actualExpression
                                          : Expression.Coalesce(actualExpression, Expression.Default(targetType));
            var resultExpression = Expression.Convert(coalescedExpression, targetType);
            var expressions = _assignments.Concat(new[] { resultExpression });
            var body = Expression.Block(_locals, expressions);
            var lambda = Expression.Lambda<Func<T>>(body);
            return lambda.Compile();
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
            var liftedInput = BuildCachedExpression(expression.Expression);
            var nullableResultType = expression.Type.GetNullableType();
            var signature = expression.Signature;

            return Expression.Condition(
                BuildNullCheck(liftedInput),
                BuildNullValue(nullableResultType),
                BuildLiftedExpression(
                    BuildUnaryExpression(
                        signature,
                        BuildLoweredExpression(liftedInput)
                    )
                )
            );
        }

        private static Expression BuildUnaryExpression(UnaryOperatorSignature unaryOperatorSignature, Expression input)
        {
            var signature = unaryOperatorSignature;

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
            var liftedLeft = BuildCachedExpression(expression.Left);
            var liftedRight = BuildCachedExpression(expression.Right);
            var nullableResultType = expression.Type.GetNullableType();
            var signature = expression.Signature;

            var result = Expression.Condition(
                            Expression.OrElse(
                                BuildNullCheck(liftedLeft),
                                BuildNullCheck(liftedRight)
                            ),
                            BuildNullValue(nullableResultType),
                            BuildLiftedExpression(
                                BuildBinaryExpression(signature,
                                                      BuildLoweredExpression(liftedLeft),
                                                      BuildLoweredExpression(liftedRight)
                                )
                            )
                         );

            // If this is is not a logical operator, we are done.

            if (signature.Kind != BinaryOperatorKind.LogicalAnd && signature.Kind != BinaryOperatorKind.LogicalOr)
                return result;

            // For logical operators we have to pay special attention to NULL values.
            //
            // Normally, a binary expression will yield NULL if any of the operands is NULL.
            //
            // For conjuctions and disjunctions this is not true. For certain values these
            // operators will return TRUE or FALSE though an operand was null. The following 
            // truth table must hold, sepcial cases are marked in parentheses:
            //
            //    AND |  F  |  T  |  N       OR |  F  |  T  |  N
            //    ----+-----+-----+----      ---+-----+-----+-----
            //     F  |  F  |  F  | (F)      F  |  F  |  T  |  N
            //     T  |  F  |  T  |  N       T  |  T  |  T  | (T)
            //     N  | (F) |  N  |  N       N  |  N  | (T) |  N
            //
            // The special cases for conjuctions and disjunctions are
            // pretty much the same:
            //
            // Logical And --> If either side is FALSE, the result is FALSE.
            // Logical Or  --> If either side is TRUE, the result is TRUE.

            var specialValue = signature.Kind == BinaryOperatorKind.LogicalOr;
            var constant = Expression.Convert(Expression.Constant(specialValue), typeof (bool?));

            return
                Expression.Condition(
                    Expression.OrElse(
                        Expression.Equal(
                            liftedLeft,
                            constant
                        ),
                        Expression.Equal(
                            liftedRight,
                            constant
                        )
                    ),
                    constant,
                    result
                );
        }

        private static Expression BuildBinaryExpression(BinaryOperatorSignature signature, Expression left, Expression right)
        {
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

        private static Expression BuildLiteralExpression(AlgebraLiteralExpression expression)
        {
            return Expression.Constant(expression.Value, expression.Type);
        }

        private Expression BuildValueSlotExpression(AlgebraValueSlotExpression expression)
        {
            var rowBufferIndex = _valueSlotSettings.GetRowBufferIndex(expression.ValueSlot);
            var rowBufferFunc = _valueSlotSettings.RowBufferProvider;
            return
                Expression.Convert(
                    Expression.MakeIndex(
                        Expression.Invoke(
                            Expression.Constant(rowBufferFunc)
                        ),
                        RowBufferIndexer,
                        new[] { Expression.Constant(rowBufferIndex) }
                    ),
                    expression.ValueSlot.Type.GetNullableType()
                );
        }

        private static Expression BuildVariableExpression(AlgebraVariableExpression expression)
        {
            return
                Expression.Convert(
                    Expression.MakeMemberAccess(
                        Expression.Constant(expression.Symbol),
                        VariableSymbolValueProperty
                    ),
                    expression.Type.GetNullableType()
                );
        }

        private Expression BuildFunctionInvocationExpression(AlgebraFunctionInvocationExpression expression)
        {
            var liftedArguments = expression.Arguments.Select(BuildCachedExpression).ToArray();
            var nullableResultType = expression.Type.GetNullableType();

            return
                Expression.Condition(
                    BuildNullCheck(liftedArguments),
                    BuildNullValue(nullableResultType),
                    BuildLiftedExpression(
                        expression.Symbol.CreateInvocation(
                            liftedArguments.Select(BuildLoweredExpression)
                        )
                    )
                );
        }

        private Expression BuildPropertyAccessExpression(AlgebraPropertyAccessExpression expression)
        {
            var liftedInstance = BuildCachedExpression(expression.Target);
            var nullableResultType = expression.Type.GetNullableType();

            return
                Expression.Condition(
                    BuildNullCheck(liftedInstance),
                    BuildNullValue(nullableResultType),
                    BuildLiftedExpression(
                        expression.Symbol.CreateInvocation(
                            BuildLoweredExpression(liftedInstance)
                        )
                    )
                );
        }

        private Expression BuildMethodInvocationExpression(AlgebraMethodInvocationExpression expression)
        {
            var liftedInstance = BuildCachedExpression(expression.Target);
            var liftedArguments = expression.Arguments.Select(BuildCachedExpression).ToArray();
            var nullableResultType = expression.Type.GetNullableType();

            return
                Expression.Condition(
                    Expression.OrElse(
                        BuildNullCheck(liftedInstance),
                        BuildNullCheck(liftedArguments)
                    ),
                    BuildNullValue(nullableResultType),
                    BuildLiftedExpression(
                        expression.Symbol.CreateInvocation(
                            BuildLoweredExpression(liftedInstance),
                            liftedArguments.Select(BuildLoweredExpression)
                        )
                    )
                );
        }

        private Expression BuildConversionExpression(AlgebraConversionExpression expression)
        {
            if (expression.Expression.Type.IsNull())
                return BuildNullValue(expression.Type);

            var input = BuildCachedExpression(expression.Expression);
            var targetType = expression.Type;
            var conversionMethod = expression.Conversion.ConversionMethods.SingleOrDefault();
            return
                Expression.Condition(
                    BuildNullCheck(input),
                    BuildNullValue(targetType),
                    BuildLiftedExpression(
                        Expression.Convert(
                            input,
                            targetType,
                            conversionMethod
                        )
                    )
                );
        }

        private Expression BuildIsNullExpression(AlgebraIsNullExpression expression)
        {
            return BuildNullCheck(BuildExpression(expression.Expression));
        }

        private Expression BuildCaseExpression(AlgebraCaseExpression expression)
        {
            return BuildCaseLabel(expression, 0);
        }

        private Expression BuildCaseLabel(AlgebraCaseExpression caseExpression, int caseLabelIndex)
        {
            if (caseLabelIndex == caseExpression.CaseLabels.Count)
                return caseExpression.ElseExpression == null
                           ? BuildNullValue(caseExpression.Type)
                           : BuildLiftedExpression(caseExpression.ElseExpression);

            var caseLabel = caseExpression.CaseLabels[caseLabelIndex];
            var condition = caseLabel.Condition;
            var result = caseLabel.Result;

            return
                Expression.Condition(
                    Expression.Equal(
                        BuildLiftedExpression(condition),
                        BuildNullableTrue()
                    ),
                    BuildLiftedExpression(result),
                    BuildCaseLabel(caseExpression, caseLabelIndex + 1)
                );
        }
    }
}