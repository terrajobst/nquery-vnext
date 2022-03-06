using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;

using NQuery.Binding;
using NQuery.Symbols;

namespace NQuery.Iterators
{
    internal sealed class ExpressionBuilder
    {
        private static readonly PropertyInfo RowBufferIndexer = typeof(RowBuffer).GetProperty("Item", new[] { typeof(int) });
        private static readonly PropertyInfo VariableSymbolValueProperty = typeof(VariableSymbol).GetProperty("Value", typeof(object));

        private readonly RowBufferAllocation _rowBufferAllocation;
        private readonly List<ParameterExpression> _locals = new();
        private readonly List<Expression> _assignments = new();

        private ExpressionBuilder(RowBufferAllocation allocation)
        {
            _rowBufferAllocation = allocation;
        }

        public static Func<object> BuildFunction(BoundExpression expression)
        {
            return BuildExpression<Func<object>>(expression, typeof(object), RowBufferAllocation.Empty);
        }

        public static IteratorFunction BuildIteratorFunction(BoundExpression expression, RowBufferAllocation allocation)
        {
            return BuildExpression<IteratorFunction>(expression, typeof(object), allocation);
        }

        public static IteratorPredicate BuildIteratorPredicate(BoundExpression predicate, bool nullValue, RowBufferAllocation allocation)
        {
            if (predicate is null)
                return () => nullValue;

            return BuildExpression<IteratorPredicate>(predicate, typeof(bool), allocation);
        }

        private static TDelegate BuildExpression<TDelegate>(BoundExpression expression, Type targetType, RowBufferAllocation allocation)
        {
            var builder = new ExpressionBuilder(allocation);
            return builder.BuildExpression<TDelegate>(expression, targetType);
        }

        private ParameterExpression BuildCachedExpression(BoundExpression expression)
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

        private Expression BuildLiftedExpression(BoundExpression expression)
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
                       ? (Expression)Expression.Not(Expression.Property(expression, nameof(Nullable<bool>.HasValue)))
                       : Expression.ReferenceEqual(expression, Expression.Constant(null, expression.Type));
        }

        private static Expression BuildNullCheck(IEnumerable<Expression> expressions)
        {
            return expressions
                .Select(BuildNullCheck)
                .Aggregate<Expression, Expression>(null, (current, nullCheck) => current is null ? nullCheck : Expression.OrElse(current, nullCheck));
        }

        private static Expression BuildNullCheck(Expression instance, IReadOnlyCollection<Expression> arguments)
        {
            if (arguments.Count == 0)
                return BuildNullCheck(instance);

            return
                Expression.OrElse(
                    BuildNullCheck(instance),
                    BuildNullCheck(arguments)
                );
        }

        private static Expression BuildInvocation(MethodSymbol methodSymbol, Expression instance, IEnumerable<Expression> arguments)
        {
            return
                BuildLiftedExpression(
                    methodSymbol.CreateInvocation(
                    BuildLoweredExpression(instance),
                        arguments.Select(BuildLoweredExpression)
                    )
                );
        }

        private static Expression BuildInvocation(FunctionSymbol functionSymbol, IEnumerable<Expression> arguments)
        {
            return
                BuildLiftedExpression(
                    functionSymbol.CreateInvocation(
                        arguments.Select(BuildLoweredExpression)
                    )
                );
        }

        private static Expression BuildInvocation(PropertySymbol propertySymbol, Expression instance)
        {
            return
                BuildLiftedExpression(
                    propertySymbol.CreateInvocation(
                        BuildLoweredExpression(instance)
                    )
                );
        }

        private static UnaryExpression BuildNullableTrue()
        {
            return Expression.Convert(Expression.Constant(true), typeof(bool?));
        }

        private TDelegate BuildExpression<TDelegate>(BoundExpression expression, Type targetType)
        {
            var actualExpression = BuildCachedExpression(expression);
            var coalescedExpression = targetType.CanBeNull()
                                          ? (Expression)actualExpression
                                          : Expression.Coalesce(actualExpression, Expression.Default(targetType));
            var resultExpression = Expression.Convert(coalescedExpression, targetType);
            var expressions = _assignments.Concat(new[] { resultExpression });
            var body = Expression.Block(_locals, expressions);
            var lambda = Expression.Lambda<TDelegate>(body);
            return lambda.Compile();
        }

        private Expression BuildExpression(BoundExpression expression)
        {
            switch (expression.Kind)
            {
                case BoundNodeKind.UnaryExpression:
                    return BuildUnaryExpression((BoundUnaryExpression)expression);
                case BoundNodeKind.BinaryExpression:
                    return BuildBinaryExpression((BoundBinaryExpression)expression);
                case BoundNodeKind.LiteralExpression:
                    return BuildLiteralExpression((BoundLiteralExpression)expression);
                case BoundNodeKind.ValueSlotExpression:
                    return BuildValueSlotExpression((BoundValueSlotExpression)expression);
                case BoundNodeKind.VariableExpression:
                    return BuildVariableExpression((BoundVariableExpression)expression);
                case BoundNodeKind.FunctionInvocationExpression:
                    return BuildFunctionInvocationExpression((BoundFunctionInvocationExpression)expression);
                case BoundNodeKind.PropertyAccessExpression:
                    return BuildPropertyAccessExpression((BoundPropertyAccessExpression)expression);
                case BoundNodeKind.MethodInvocationExpression:
                    return BuildMethodInvocationExpression((BoundMethodInvocationExpression)expression);
                case BoundNodeKind.ConversionExpression:
                    return BuildConversionExpression((BoundConversionExpression)expression);
                case BoundNodeKind.IsNullExpression:
                    return BuildIsNullExpression((BoundIsNullExpression)expression);
                case BoundNodeKind.CaseExpression:
                    return BuildCaseExpression((BoundCaseExpression)expression);
                default:
                    throw ExceptionBuilder.UnexpectedValue(expression.Kind);
            }
        }

        private Expression BuildUnaryExpression(BoundUnaryExpression expression)
        {
            var liftedInput = BuildCachedExpression(expression.Expression);
            var nullableResultType = expression.Type.GetNullableType();
            var signature = expression.Result.Best.Signature;

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
                    throw ExceptionBuilder.UnexpectedValue(signature.Kind);
            }
        }

        private Expression BuildBinaryExpression(BoundBinaryExpression expression)
        {
            var liftedLeft = BuildCachedExpression(expression.Left);
            var liftedRight = BuildCachedExpression(expression.Right);
            var nullableResultType = expression.Type.GetNullableType();
            var signature = expression.Result.Best.Signature;

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

            // If this is not a logical operator, we are done.

            if (signature.Kind != BinaryOperatorKind.LogicalAnd && signature.Kind != BinaryOperatorKind.LogicalOr)
                return result;

            // For logical operators we have to pay special attention to NULL values.
            //
            // Normally, a binary expression will yield NULL if any of the operands is NULL.
            //
            // For conjunctions and disjunctions this is not true. For certain values these
            // operators will return TRUE or FALSE though an operand was null. The following
            // truth table must hold, special cases are marked in parentheses:
            //
            //    AND |  F  |  T  |  N       OR |  F  |  T  |  N
            //    ----+-----+-----+----      ---+-----+-----+-----
            //     F  |  F  |  F  | (F)      F  |  F  |  T  |  N
            //     T  |  F  |  T  |  N       T  |  T  |  T  | (T)
            //     N  | (F) |  N  |  N       N  |  N  | (T) |  N
            //
            // The special cases for conjunctions and disjunctions are
            // pretty much the same:
            //
            // Logical And --> If either side is FALSE, the result is FALSE.
            // Logical Or  --> If either side is TRUE, the result is TRUE.

            var specialValue = signature.Kind == BinaryOperatorKind.LogicalOr;
            var constant = Expression.Convert(Expression.Constant(specialValue), typeof(bool?));

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
                case BinaryOperatorKind.SoundsLike:
                    return Expression.Call(signature.MethodInfo, left, right);
                default:
                    throw ExceptionBuilder.UnexpectedValue(signature.Kind);
            }
        }

        private static Expression BuildLiteralExpression(BoundLiteralExpression expression)
        {
            return
                BuildLiftedExpression(
                    Expression.Constant(expression.Value, expression.Type)
                );
        }

        private Expression BuildValueSlotExpression(BoundValueSlotExpression expression)
        {
            var entry = _rowBufferAllocation[expression.ValueSlot];

            return
                Expression.Convert(
                    Expression.MakeIndex(
                        Expression.Constant(entry.RowBuffer),
                        RowBufferIndexer,
                        new[] { Expression.Constant(entry.Index) }
                    ),
                    expression.ValueSlot.Type.GetNullableType()
                );
        }

        private static Expression BuildVariableExpression(BoundVariableExpression expression)
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

        private Expression BuildFunctionInvocationExpression(BoundFunctionInvocationExpression expression)
        {
            var liftedArguments = expression.Arguments.Select(BuildCachedExpression).ToImmutableArray();
            if (liftedArguments.Length == 0)
                return BuildInvocation(expression.Symbol, liftedArguments);

            var nullableResultType = expression.Type.GetNullableType();
            return
                Expression.Condition(
                    BuildNullCheck(liftedArguments),
                    BuildNullValue(nullableResultType),
                    BuildInvocation(expression.Symbol, liftedArguments)
                );
        }

        private Expression BuildPropertyAccessExpression(BoundPropertyAccessExpression expression)
        {
            var liftedInstance = BuildCachedExpression(expression.Target);
            var nullableResultType = expression.Type.GetNullableType();

            return
                Expression.Condition(
                    BuildNullCheck(liftedInstance),
                    BuildNullValue(nullableResultType),
                    BuildInvocation(expression.Symbol, liftedInstance)
                );
        }

        private Expression BuildMethodInvocationExpression(BoundMethodInvocationExpression expression)
        {
            var liftedInstance = BuildCachedExpression(expression.Target);
            var liftedArguments = expression.Arguments.Select(BuildCachedExpression).ToImmutableArray();
            var nullableResultType = expression.Type.GetNullableType();

            return
                Expression.Condition(
                    BuildNullCheck(liftedInstance, liftedArguments),
                    BuildNullValue(nullableResultType),
                    BuildInvocation(expression.Symbol, liftedInstance, liftedArguments)
                );
        }

        private Expression BuildConversionExpression(BoundConversionExpression expression)
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

        private Expression BuildIsNullExpression(BoundIsNullExpression expression)
        {
            return BuildNullCheck(BuildExpression(expression.Expression));
        }

        private Expression BuildCaseExpression(BoundCaseExpression expression)
        {
            return BuildCaseLabel(expression, 0);
        }

        private Expression BuildCaseLabel(BoundCaseExpression caseExpression, int caseLabelIndex)
        {
            if (caseLabelIndex == caseExpression.CaseLabels.Length)
                return caseExpression.ElseExpression is null
                           ? BuildNullValue(caseExpression.Type)
                           : BuildLiftedExpression(caseExpression.ElseExpression);

            var caseLabel = caseExpression.CaseLabels[caseLabelIndex];
            var condition = caseLabel.Condition;
            var result = caseLabel.ThenExpression;

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