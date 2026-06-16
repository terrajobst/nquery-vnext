using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;

using NQuery.CodeAnalysis.Algebra;
using NQuery.CodeAnalysis.Binding;
using NQuery.CodeAnalysis.Symbols;

namespace NQuery.CodeAnalysis.Iterators;

// Compiles a LogicalExpression into a delegate that takes the row buffer as a
// parameter. A value-slot reference indexes that parameter buffer at a position
// determined statically from the operator's output slot order -- nothing about a
// particular execution is baked in, so the delegate is compiled once and reused.
//
// The three-valued-logic handling mirrors ExpressionBuilder/ScalarEmitter; the
// only difference is how value slots resolve.
internal sealed class EmittedExpressionCompiler
{
    private static readonly PropertyInfo RowBufferIndexer = typeof(RowBuffer).GetProperty("Item", new[] { typeof(int) })!;
    private static readonly PropertyInfo VariableSymbolValueProperty = typeof(VariableSymbol).GetProperty("Value", typeof(object))!;

    private readonly FrozenDictionary<ValueSlot, int> _slotIndices;
    private readonly ParameterExpression _rowBuffer = Expression.Parameter(typeof(RowBuffer));
    private readonly List<ParameterExpression> _locals = new();
    private readonly List<Expression> _assignments = new();

    private EmittedExpressionCompiler(FrozenDictionary<ValueSlot, int> slotIndices)
    {
        _slotIndices = slotIndices;
    }

    // The row buffer's column layout follows the producing operator's output slot
    // order, so a slot's runtime index is just its ordinal there.
    public static FrozenDictionary<ValueSlot, int> CreateSlotIndices(ImmutableArray<ValueSlot> outputValueSlots)
    {
        var map = new Dictionary<ValueSlot, int>(outputValueSlots.Length);
        for (var i = 0; i < outputValueSlots.Length; i++)
        {
            if (!map.ContainsKey(outputValueSlots[i]))
                map.Add(outputValueSlots[i], i);
        }

        return map.ToFrozenDictionary();
    }

    public static EmittedFunction CompileFunction(LogicalExpression expression, FrozenDictionary<ValueSlot, int> slotIndices)
    {
        return Compile<EmittedFunction>(expression, typeof(object), slotIndices);
    }

    public static EmittedPredicate CompilePredicate(LogicalExpression expression, FrozenDictionary<ValueSlot, int> slotIndices)
    {
        return Compile<EmittedPredicate>(expression, typeof(bool), slotIndices);
    }

    private static TDelegate Compile<TDelegate>(LogicalExpression expression, Type targetType, FrozenDictionary<ValueSlot, int> slotIndices) where TDelegate : Delegate
    {
        var compiler = new EmittedExpressionCompiler(slotIndices);
        var lambda = compiler.BuildLambda(expression, typeof(TDelegate), targetType);
        return (TDelegate)lambda.Compile();
    }

    private LambdaExpression BuildLambda(LogicalExpression expression, Type delegateType, Type targetType)
    {
        var actualExpression = BuildCachedExpression(expression);
        var coalescedExpression = targetType.CanBeNull()
                                      ? actualExpression
                                      : Expression.Coalesce(actualExpression, Expression.Default(targetType));
        var resultExpression = Expression.Convert(coalescedExpression, targetType);
        var expressions = _assignments.Concat(new[] { resultExpression });
        var body = Expression.Block(_locals, expressions);
        return Expression.Lambda(delegateType, body, _rowBuffer);
    }

    private Expression BuildCachedExpression(LogicalExpression expression)
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

    private static Expression BuildLoweredExpression(Expression expression)
    {
        if (!expression.Type.IsNullableOfT())
            return expression;

        return Expression.Convert(expression, expression.Type.GetNonNullableType());
    }

    private static Expression BuildNullValue(Type type)
    {
        return Expression.Constant(null, type.GetNullableType());
    }

    private static Expression BuildNullCheck(Expression expression)
    {
        return expression.Type.IsNullableOfT()
                   ? Expression.Not(Expression.Property(expression, nameof(Nullable<bool>.HasValue)))
                   : Expression.ReferenceEqual(expression, Expression.Constant(null, expression.Type));
    }

    private static Expression BuildNullCheck(IEnumerable<Expression> expressions)
    {
        return expressions
            .Select(BuildNullCheck)
            .Aggregate<Expression, Expression>(null!, (current, nullCheck) => current is null ? nullCheck : Expression.OrElse(current, nullCheck));
    }

    private static Expression BuildNullCheck(Expression instance, IReadOnlyCollection<Expression> arguments)
    {
        if (arguments.Count == 0)
            return BuildNullCheck(instance);

        return Expression.OrElse(BuildNullCheck(instance), BuildNullCheck(arguments));
    }

    private static Expression BuildInvocation(MethodSymbol methodSymbol, Expression instance, IEnumerable<Expression> arguments)
    {
        return BuildLiftedExpression(methodSymbol.CreateInvocation(BuildLoweredExpression(instance), arguments.Select(BuildLoweredExpression)));
    }

    private static Expression BuildInvocation(FunctionSymbol functionSymbol, IEnumerable<Expression> arguments)
    {
        return BuildLiftedExpression(functionSymbol.CreateInvocation(arguments.Select(BuildLoweredExpression)));
    }

    private static Expression BuildInvocation(PropertySymbol propertySymbol, Expression instance)
    {
        return BuildLiftedExpression(propertySymbol.CreateInvocation(BuildLoweredExpression(instance)));
    }

    private static UnaryExpression BuildNullableTrue()
    {
        return Expression.Convert(Expression.Constant(true), typeof(bool?));
    }

    private Expression BuildExpression(LogicalExpression expression)
    {
        switch (expression.Kind)
        {
            case LogicalExpressionKind.Unary:
                return BuildUnaryExpression((LogicalUnaryExpression)expression);
            case LogicalExpressionKind.Binary:
                return BuildBinaryExpression((LogicalBinaryExpression)expression);
            case LogicalExpressionKind.Literal:
                return BuildLiteralExpression((LogicalLiteralExpression)expression);
            case LogicalExpressionKind.ValueSlot:
                return BuildValueSlotExpression((LogicalValueSlotExpression)expression);
            case LogicalExpressionKind.Variable:
                return BuildVariableExpression((LogicalVariableExpression)expression);
            case LogicalExpressionKind.FunctionInvocation:
                return BuildFunctionInvocationExpression((LogicalFunctionInvocationExpression)expression);
            case LogicalExpressionKind.PropertyAccess:
                return BuildPropertyAccessExpression((LogicalPropertyAccessExpression)expression);
            case LogicalExpressionKind.MethodInvocation:
                return BuildMethodInvocationExpression((LogicalMethodInvocationExpression)expression);
            case LogicalExpressionKind.Conversion:
                return BuildConversionExpression((LogicalConversionExpression)expression);
            case LogicalExpressionKind.IsNull:
                return BuildIsNullExpression((LogicalIsNullExpression)expression);
            case LogicalExpressionKind.Case:
                return BuildCaseExpression((LogicalCaseExpression)expression);
            default:
                throw ExceptionBuilder.UnexpectedValue(expression.Kind);
        }
    }

    private Expression BuildUnaryExpression(LogicalUnaryExpression expression)
    {
        var liftedInput = BuildCachedExpression(expression.Expression);
        var nullableResultType = expression.Type.GetNullableType();
        var signature = expression.Result.Best!.Signature;

        return Expression.Condition(
            BuildNullCheck(liftedInput),
            BuildNullValue(nullableResultType),
            BuildLiftedExpression(BuildUnaryExpression(signature, BuildLoweredExpression(liftedInput)))
        );
    }

    private static Expression BuildUnaryExpression(UnaryOperatorSignature signature, Expression input)
    {
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

    private Expression BuildBinaryExpression(LogicalBinaryExpression expression)
    {
        var liftedLeft = BuildCachedExpression(expression.Left);
        var liftedRight = BuildCachedExpression(expression.Right);
        var nullableResultType = expression.Type.GetNullableType();
        var signature = expression.Result.Best!.Signature;

        var result = Expression.Condition(
                        Expression.OrElse(BuildNullCheck(liftedLeft), BuildNullCheck(liftedRight)),
                        BuildNullValue(nullableResultType),
                        BuildLiftedExpression(BuildBinaryExpression(signature, BuildLoweredExpression(liftedLeft), BuildLoweredExpression(liftedRight)))
                     );

        if (signature.Kind != BinaryOperatorKind.LogicalAnd && signature.Kind != BinaryOperatorKind.LogicalOr)
            return result;

        // AND yields FALSE if either side is FALSE; OR yields TRUE if either side
        // is TRUE -- even when the other operand is NULL.
        var specialValue = signature.Kind == BinaryOperatorKind.LogicalOr;
        var constant = Expression.Convert(Expression.Constant(specialValue), typeof(bool?));

        return
            Expression.Condition(
                Expression.OrElse(Expression.Equal(liftedLeft, constant), Expression.Equal(liftedRight, constant)),
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
                return Expression.Call(signature.MethodInfo!, left, right);
            default:
                throw ExceptionBuilder.UnexpectedValue(signature.Kind);
        }
    }

    private static Expression BuildLiteralExpression(LogicalLiteralExpression expression)
    {
        return BuildLiftedExpression(Expression.Constant(expression.Value, expression.Type));
    }

    private Expression BuildValueSlotExpression(LogicalValueSlotExpression expression)
    {
        var index = _slotIndices[expression.ValueSlot];

        return
            Expression.Convert(
                Expression.MakeIndex(_rowBuffer, RowBufferIndexer, new[] { Expression.Constant(index) }),
                expression.ValueSlot.Type.GetNullableType()
            );
    }

    private static Expression BuildVariableExpression(LogicalVariableExpression expression)
    {
        return
            Expression.Convert(
                Expression.MakeMemberAccess(Expression.Constant(expression.Symbol), VariableSymbolValueProperty),
                expression.Type.GetNullableType()
            );
    }

    private Expression BuildFunctionInvocationExpression(LogicalFunctionInvocationExpression expression)
    {
        var liftedArguments = expression.Arguments.Select(BuildCachedExpression).ToImmutableArray();
        if (liftedArguments.Length == 0)
            return BuildInvocation(expression.Symbol!, liftedArguments);

        return
            Expression.Condition(
                BuildNullCheck(liftedArguments),
                BuildNullValue(expression.Type.GetNullableType()),
                BuildInvocation(expression.Symbol!, liftedArguments)
            );
    }

    private Expression BuildPropertyAccessExpression(LogicalPropertyAccessExpression expression)
    {
        var liftedInstance = BuildCachedExpression(expression.Target);

        return
            Expression.Condition(
                BuildNullCheck(liftedInstance),
                BuildNullValue(expression.Type.GetNullableType()),
                BuildInvocation(expression.Symbol, liftedInstance)
            );
    }

    private Expression BuildMethodInvocationExpression(LogicalMethodInvocationExpression expression)
    {
        var liftedInstance = BuildCachedExpression(expression.Target);
        var liftedArguments = expression.Arguments.Select(BuildCachedExpression).ToImmutableArray();

        return
            Expression.Condition(
                BuildNullCheck(liftedInstance, liftedArguments),
                BuildNullValue(expression.Type.GetNullableType()),
                BuildInvocation(expression.Symbol!, liftedInstance, liftedArguments)
            );
    }

    private Expression BuildConversionExpression(LogicalConversionExpression expression)
    {
        if (expression.Expression.Type.IsNull())
            return BuildNullValue(expression.Type);

        var input = BuildCachedExpression(expression.Expression);
        var conversionMethod = expression.Conversion.ConversionMethods.SingleOrDefault();
        return
            Expression.Condition(
                BuildNullCheck(input),
                BuildNullValue(expression.Type),
                BuildLiftedExpression(Expression.Convert(input, expression.Type, conversionMethod))
            );
    }

    private Expression BuildIsNullExpression(LogicalIsNullExpression expression)
    {
        return BuildNullCheck(BuildExpression(expression.Expression));
    }

    private Expression BuildCaseExpression(LogicalCaseExpression expression)
    {
        return BuildCaseLabel(expression, 0);
    }

    private Expression BuildCaseLabel(LogicalCaseExpression caseExpression, int caseLabelIndex)
    {
        if (caseLabelIndex == caseExpression.CaseLabels.Length)
            return caseExpression.ElseExpression is null
                       ? BuildNullValue(caseExpression.Type)
                       : BuildNestedScopeInvocation(caseExpression.ElseExpression);

        var caseLabel = caseExpression.CaseLabels[caseLabelIndex];

        return
            Expression.Condition(
                Expression.Equal(BuildNestedScopeInvocation(caseLabel.Condition), BuildNullableTrue()),
                BuildNestedScopeInvocation(caseLabel.ThenExpression),
                BuildCaseLabel(caseExpression, caseLabelIndex + 1)
            );
    }

    private Expression BuildNestedScopeInvocation(LogicalExpression expression)
    {
        var targetType = expression.Type;
        var delegateType = typeof(Func<,>).MakeGenericType(typeof(RowBuffer), targetType);

        var nested = new EmittedExpressionCompiler(_slotIndices);
        var lambda = nested.BuildLambda(expression, delegateType, targetType);

        // The nested scope reads from the same row buffer; pass it through.
        var invocation = Expression.Invoke(lambda, _rowBuffer);
        return BuildLiftedExpression(invocation);
    }
}
