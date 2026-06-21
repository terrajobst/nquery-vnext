using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;

using NQuery.CodeAnalysis.Algebra;
using NQuery.CodeAnalysis.Binding;
using NQuery.CodeAnalysis.Iterators;
using NQuery.CodeAnalysis.Symbols;

namespace NQuery.CodeAnalysis.Emit;

// Compiles a LogicalExpression into a delegate that takes the row buffer as a
// parameter. A value-slot reference indexes that parameter buffer at a position
// determined statically from the operator's output slot order -- nothing about a
// particular execution is baked in, so the delegate is compiled once and reused.
//
// NULLs follow SQL three-valued logic: an operand that is NULL generally makes the
// whole result NULL, except where AND/OR can short-circuit to FALSE/TRUE.
internal sealed class ExpressionCompiler
{
    private static readonly PropertyInfo VariableSymbolValueProperty = typeof(VariableSymbol).GetProperty("Value", typeof(object))!;

    private static readonly MethodInfo ReadObjectMethod = typeof(RowBuffer).GetMethod(nameof(RowBuffer.ReadObject))!;
    private static readonly MethodInfo Read32BitMethod = typeof(RowBuffer).GetMethod(nameof(RowBuffer.Read32Bit))!;
    private static readonly MethodInfo Read64BitMethod = typeof(RowBuffer).GetMethod(nameof(RowBuffer.Read64Bit))!;
    private static readonly MethodInfo Read128BitMethod = typeof(RowBuffer).GetMethod(nameof(RowBuffer.Read128Bit))!;

    private static readonly MethodInfo WriteObjectMethod = typeof(ArrayRowBuffer).GetMethod(nameof(ArrayRowBuffer.WriteObject))!;
    private static readonly MethodInfo Write32BitMethod = typeof(ArrayRowBuffer).GetMethod(nameof(ArrayRowBuffer.Write32Bit))!;
    private static readonly MethodInfo Write64BitMethod = typeof(ArrayRowBuffer).GetMethod(nameof(ArrayRowBuffer.Write64Bit))!;
    private static readonly MethodInfo Write128BitMethod = typeof(ArrayRowBuffer).GetMethod(nameof(ArrayRowBuffer.Write128Bit))!;

    private readonly FrozenDictionary<ValueSlot, RowBufferColumn> _slotIndices;
    private readonly ParameterExpression _rowBuffer;
    private readonly List<ParameterExpression> _locals = new();
    private readonly List<Expression> _assignments = new();

    private ExpressionCompiler(FrozenDictionary<ValueSlot, RowBufferColumn> slotIndices)
        : this(slotIndices, Expression.Parameter(typeof(RowBuffer)))
    {
    }

    // Several values can be compiled against one shared input-buffer parameter so their
    // bodies fold into a single lambda (see CompileRowWriter); each compiler still keeps
    // its own locals/assignments, so the per-value blocks stay independent.
    private ExpressionCompiler(FrozenDictionary<ValueSlot, RowBufferColumn> slotIndices, ParameterExpression rowBuffer)
    {
        _slotIndices = slotIndices;
        _rowBuffer = rowBuffer;
    }

    // The row buffer's column layout follows the producing operator's output slot
    // order, so a slot resolves to its container-relative address in that layout.
    public static FrozenDictionary<ValueSlot, RowBufferColumn> CreateSlotIndices(ImmutableArray<ValueSlot> outputValueSlots)
    {
        return RowBufferLayout.CreateSlotMap(outputValueSlots);
    }

    public static CompiledFunction CompileFunction(LogicalExpression expression, FrozenDictionary<ValueSlot, RowBufferColumn> slotIndices)
    {
        return Compile<CompiledFunction>(expression, typeof(object), slotIndices);
    }

    public static CompiledPredicate CompilePredicate(LogicalExpression expression, FrozenDictionary<ValueSlot, RowBufferColumn> slotIndices)
    {
        return Compile<CompiledPredicate>(expression, typeof(bool), slotIndices);
    }

    // Compiles a whole set of computed values into one typed store: the per-value loop is
    // unrolled into a single straight-line block that writes each result into its column
    // (never boxed through object), so a computed row costs one delegate invocation instead
    // of one per value. Used by the compute scalar to fill its computed columns. Each value
    // gets its own compiler -- so its locals stay scoped -- but they all share the input and
    // target buffer parameters so the bodies fold into a single lambda.
    public static Action<RowBuffer, ArrayRowBuffer> CompileRowWriter(ImmutableArray<LogicalComputedValue> definedValues, RowBufferLayout computedLayout, FrozenDictionary<ValueSlot, RowBufferColumn> slotIndices)
    {
        var rowBuffer = Expression.Parameter(typeof(RowBuffer));
        var targetBuffer = Expression.Parameter(typeof(ArrayRowBuffer));

        var writes = new Expression[definedValues.Length];
        for (var i = 0; i < definedValues.Length; i++)
        {
            var slotType = definedValues[i].ValueSlot.Type;
            var compiler = new ExpressionCompiler(slotIndices, rowBuffer);
            var value = compiler.BuildBody(definedValues[i].Expression, slotType.GetNullableType());
            writes[i] = BuildWriteCall(targetBuffer, computedLayout.Columns[i], slotType, value);
        }

        var body = writes.Length == 0 ? (Expression)Expression.Empty() : Expression.Block(writes);
        return Expression.Lambda<Action<RowBuffer, ArrayRowBuffer>>(body, rowBuffer, targetBuffer).Compile();
    }

    private static TDelegate Compile<TDelegate>(LogicalExpression expression, Type targetType, FrozenDictionary<ValueSlot, RowBufferColumn> slotIndices) where TDelegate : Delegate
    {
        var compiler = new ExpressionCompiler(slotIndices);
        var lambda = compiler.BuildLambda(expression, typeof(TDelegate), targetType);
        return (TDelegate)lambda.Compile();
    }

    private LambdaExpression BuildLambda(LogicalExpression expression, Type delegateType, Type targetType)
    {
        var body = BuildBody(expression, targetType);
        return Expression.Lambda(delegateType, body, _rowBuffer);
    }

    // Builds a call that stores `value` (already the column's nullable type) into the
    // target column. Shared by the compute-scalar writer, the table-scan column writers,
    // and the aggregate result store.
    public static Expression BuildWriteCall(Expression targetBuffer, RowBufferColumn target, Type slotType, Expression value)
    {
        var index = Expression.Constant(target.Index);
        return target.Kind switch
        {
            RowBufferColumnKind.Object => Expression.Call(targetBuffer, WriteObjectMethod.MakeGenericMethod(slotType.GetNullableType()), index, value),
            RowBufferColumnKind.Bits32 => Expression.Call(targetBuffer, Write32BitMethod.MakeGenericMethod(slotType), index, value),
            RowBufferColumnKind.Bits64 => Expression.Call(targetBuffer, Write64BitMethod.MakeGenericMethod(slotType), index, value),
            RowBufferColumnKind.Bits128 => Expression.Call(targetBuffer, Write128BitMethod.MakeGenericMethod(slotType), index, value),
            _ => throw ExceptionBuilder.UnexpectedValue(target.Kind)
        };
    }

    private Expression BuildBody(LogicalExpression expression, Type targetType)
    {
        var actualExpression = BuildCachedExpression(expression);

        // A bare NULL literal carries the NullType sentinel, which has no coercion to a
        // value type's nullable form -- its value is simply null (or the coalesced
        // default for a non-nullable target, matching a NULL predicate's FALSE).
        Expression resultExpression;
        if (actualExpression.Type.IsNull())
        {
            resultExpression = targetType.CanBeNull()
                                   ? Expression.Constant(null, targetType)
                                   : Expression.Default(targetType);
        }
        else
        {
            var coalescedExpression = targetType.CanBeNull()
                                          ? actualExpression
                                          : Expression.Coalesce(actualExpression, Expression.Default(targetType));
            resultExpression = Expression.Convert(coalescedExpression, targetType);
        }

        var expressions = _assignments.Concat(new[] { resultExpression });
        return Expression.Block(_locals, expressions);
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
        var slot = expression.ValueSlot;
        var column = _slotIndices[slot];
        var index = Expression.Constant(column.Index);

        // The typed read returns the slot's value already lifted to its nullable shape
        // (Nullable<T> for value types, a possibly-null reference otherwise), so no
        // boxing convert is needed -- the rest of the builder lifts/lowers from here.
        return column.Kind switch
        {
            RowBufferColumnKind.Object => Expression.Call(_rowBuffer, ReadObjectMethod.MakeGenericMethod(slot.Type.GetNullableType()), index),
            RowBufferColumnKind.Bits32 => Expression.Call(_rowBuffer, Read32BitMethod.MakeGenericMethod(slot.Type), index),
            RowBufferColumnKind.Bits64 => Expression.Call(_rowBuffer, Read64BitMethod.MakeGenericMethod(slot.Type), index),
            RowBufferColumnKind.Bits128 => Expression.Call(_rowBuffer, Read128BitMethod.MakeGenericMethod(slot.Type), index),
            _ => throw ExceptionBuilder.UnexpectedValue(column.Kind)
        };
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

        var nested = new ExpressionCompiler(_slotIndices);
        var lambda = nested.BuildLambda(expression, delegateType, targetType);

        // The nested scope reads from the same row buffer; pass it through.
        var invocation = Expression.Invoke(lambda, _rowBuffer);
        return BuildLiftedExpression(invocation);
    }
}
