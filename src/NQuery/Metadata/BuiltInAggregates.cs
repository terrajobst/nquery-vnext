using System.Collections.Immutable;
using System.Globalization;
using System.Linq.Expressions;

using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Binding;

namespace NQuery.Metadata;

// The built-in aggregates expressed against the public AggregateDefinition.Create /
// AggregateFold API instead of hand-written IAggregatable/IAggregator classes. Each aggregate
// is a (seed, accumulate, result) triple of expression trees. NQuery contributes only operator
// resolution (Binary/ResolveBinaryType) and conversion (ConvertValue); those throw
// AggregateNotApplicableException when an argument type isn't supported, which unwinds the whole
// build and FoldAggregateDefinition turns into "unsupported". Everything else -- control flow,
// IComparable, string building -- is plain System.Linq.Expressions.
internal static class BuiltInAggregates
{
    // -----------------------------
    // --- Aggregate definitions ---
    // -----------------------------

    // Count and Any are also used directly by the algebrizer (cardinality guard and
    // scalar-subquery collapse), which wraps them in a symbol on demand.
    public static AggregateDefinition Count { get; } = AggregateDefinition.Create("COUNT", _ =>
    {
        var count = Expression.Parameter(typeof(int), "count");
        var value = Expression.Parameter(typeof(object), "value");
        return new AggregateFold(
            Expression.Lambda(Expression.Constant(0)),
            Expression.Lambda(Expression.Add(count, Expression.Constant(1)), count, value),
            Expression.Lambda(count, count));
    });

    // ANY is used internally (correlated subqueries), not registered as a callable aggregate.
    public static AggregateDefinition Any { get; } = AggregateDefinition.Create("ANY", argumentType =>
    {
        var stateType = MakeNullable(argumentType);
        var current = Expression.Parameter(stateType, "current");
        var value = Expression.Parameter(argumentType, "value");
        return new AggregateFold(
            Expression.Lambda(NullConstant(stateType)),
            Expression.Lambda(Cast(value, stateType), current, value),
            Expression.Lambda(current, current));
    });

    public static AggregateDefinition Sum { get; } = AggregateDefinition.Create("SUM", argumentType =>
    {
        var sumType = ResolveBinaryType(BinaryOperatorKind.Add, argumentType, argumentType);
        var stateType = MakeNullable(sumType);
        var state = Expression.Parameter(stateType, "sum");
        var value = Expression.Parameter(argumentType, "value");

        var firstValue = Cast(ConvertValue(value, sumType), stateType);
        var runningSum = Cast(Binary(BinaryOperatorKind.Add, Cast(state, sumType), value), stateType);

        return new AggregateFold(
            Expression.Lambda(NullConstant(stateType)),
            Expression.Lambda(Expression.Condition(IsNull(state), firstValue, runningSum), state, value),
            Expression.Lambda(state, state));
    });

    public static AggregateDefinition Avg { get; } = AggregateDefinition.Create("AVG", argumentType =>
    {
        var sumType = ResolveBinaryType(BinaryOperatorKind.Add, argumentType, argumentType);
        var quotientType = ResolveBinaryType(BinaryOperatorKind.Divide, sumType, typeof(int));
        var resultType = MakeNullable(quotientType);
        var nullableSumType = MakeNullable(sumType);
        var stateType = typeof(AvgState<>).MakeGenericType(sumType);   // AvgState<TSum>, Sum typed as TSum?

        var state = Expression.Parameter(stateType, "state");
        var value = Expression.Parameter(argumentType, "value");
        var sum = Expression.Field(state, "Sum");     // TSum?
        var cnt = Expression.Field(state, "Count");   // int

        var firstSum = Cast(ConvertValue(value, sumType), nullableSumType);
        var nextSum = Cast(Binary(BinaryOperatorKind.Add, Cast(sum, sumType), value), nullableSumType);
        var accBody = Expression.New(
            stateType.GetConstructor([nullableSumType, typeof(int)])!,
            Expression.Condition(IsNull(sum), firstSum, nextSum),
            Expression.Add(cnt, Expression.Constant(1)));

        var quotient = Cast(Binary(BinaryOperatorKind.Divide, Cast(sum, sumType), cnt), resultType);
        var resultBody = Expression.Condition(IsNull(sum), NullConstant(resultType), quotient);

        return new AggregateFold(
            Expression.Lambda(Expression.New(stateType)),
            Expression.Lambda(accBody, state, value),
            Expression.Lambda(resultBody, state));
    });

    public static AggregateDefinition Min { get; } = AggregateDefinition.Create("MIN", argumentType => MinMax(argumentType, isMin: true));

    public static AggregateDefinition Max { get; } = AggregateDefinition.Create("MAX", argumentType => MinMax(argumentType, isMin: false));

    public static AggregateDefinition Var { get; } = AggregateDefinition.Create("VAR", argumentType => VarStdDev(argumentType, isVar: true));

    public static AggregateDefinition StdDev { get; } = AggregateDefinition.Create("STDEV", argumentType => VarStdDev(argumentType, isVar: false));

    public static AggregateDefinition Concat { get; } = AggregateDefinition.Create("CONCAT", argumentType =>
    {
        var state = Expression.Parameter(typeof(SortedSet<string>), "values");
        var value = Expression.Parameter(argumentType, "value");

        var toString = typeof(System.Convert).GetMethod(nameof(System.Convert.ToString), [typeof(object), typeof(IFormatProvider)])!;
        var add = typeof(SortedSet<string>).GetMethod(nameof(SortedSet<string>.Add), [typeof(string)])!;
        var trim = typeof(string).GetMethod(nameof(string.Trim), Type.EmptyTypes)!;
        var join = typeof(string).GetMethod(nameof(string.Join), [typeof(string), typeof(IEnumerable<string>)])!;

        // var text = Convert.ToString(value, invariant); if (text is not null) values.Add(text.Trim()); return values
        var text = Expression.Variable(typeof(string), "text");
        var accumulate = Expression.Block(
            new[] { text },
            Expression.Assign(text, Expression.Call(toString, Expression.Convert(value, typeof(object)),
                Expression.Constant(CultureInfo.InvariantCulture, typeof(IFormatProvider)))),
            Expression.IfThen(
                Expression.ReferenceNotEqual(text, Expression.Constant(null, typeof(string))),
                Expression.Block(typeof(void), Expression.Call(state, add, Expression.Call(text, trim)))),
            state);

        return new AggregateFold(
            Expression.Lambda(Expression.New(typeof(SortedSet<string>))),
            Expression.Lambda(accumulate, state, value),
            Expression.Lambda(Expression.Call(join, Expression.Constant(", "), state), state));
    });

    public static ImmutableList<AggregateDefinition> Aggregates { get; } =
    [
        Count,
        Avg,
        Max,
        Min,
        Sum,
        StdDev,
        Var,
        Concat
    ];

    // -------------------------------------------------------------
    // --- Shared fold builders for the parameterized aggregates ---
    // -------------------------------------------------------------

    private static AggregateFold MinMax(Type argumentType, bool isMin)
    {
        if (!typeof(IComparable).IsAssignableFrom(argumentType))
            throw new AggregateNotApplicableException();

        var stateType = MakeNullable(argumentType);
        var state = Expression.Parameter(stateType, "best");
        var value = Expression.Parameter(argumentType, "value");

        var compareTo = typeof(IComparable).GetMethod(nameof(IComparable.CompareTo), [typeof(object)])!;
        var comparison = Expression.Call(
            Expression.Convert(value, typeof(IComparable)),
            compareTo,
            Expression.Convert(Cast(state, argumentType), typeof(object)));
        var takeValue = isMin
            ? Expression.LessThan(comparison, Expression.Constant(0))
            : Expression.GreaterThan(comparison, Expression.Constant(0));
        var pick = Expression.Condition(takeValue, Cast(value, stateType), state);

        return new AggregateFold(
            Expression.Lambda(NullConstant(stateType)),
            Expression.Lambda(Expression.Condition(IsNull(state), Cast(value, stateType), pick), state, value),
            Expression.Lambda(state, state));
    }

    private static AggregateFold VarStdDev(Type argumentType, bool isVar)
    {
        if (!IsNumeric(argumentType))
            throw new AggregateNotApplicableException();

        var state = Expression.Parameter(typeof(VarState), "state");
        var value = Expression.Parameter(argumentType, "value");
        var sum = Expression.Field(state, nameof(VarState.Sum));                   // decimal
        var sumOfSquares = Expression.Field(state, nameof(VarState.SumOfSquares)); // decimal
        var count = Expression.Field(state, nameof(VarState.Count));               // int

        // d = (decimal)value; new VarState(Sum + d, SumOfSquares + d*d, Count + 1)
        var d = Expression.Variable(typeof(decimal), "d");
        var accumulate = Expression.Block(
            new[] { d },
            Expression.Assign(d, ConvertValue(value, typeof(decimal))),
            Expression.New(
                typeof(VarState).GetConstructor([typeof(decimal), typeof(decimal), typeof(int)])!,
                Binary(BinaryOperatorKind.Add, sum, d),
                Binary(BinaryOperatorKind.Add, sumOfSquares, Binary(BinaryOperatorKind.Multiply, d, d)),
                Expression.Add(count, Expression.Constant(1))));

        // Count < 2 -> NULL; otherwise the variance (square-rooted for STDEV). e = Sum/Count is
        // only evaluated in the else branch, so an empty group never divides by Count == 0.
        var e = Expression.Variable(typeof(decimal), "e");
        var assignE = Expression.Assign(e, Binary(BinaryOperatorKind.Divide, sum, count));
        var enoughRows = Expression.GreaterThanOrEqual(count, Expression.Constant(2));
        var sqrt = typeof(Math).GetMethod(nameof(Math.Sqrt), [typeof(double)])!;

        var resultBody = isVar
            ? Expression.Condition(enoughRows,
                Expression.Block(new[] { e }, assignE, Cast(Variance(sum, sumOfSquares, count, e), typeof(decimal?))),
                NullConstant(typeof(decimal?)))
            : Expression.Condition(enoughRows,
                Expression.Block(new[] { e }, assignE, Cast(Expression.Call(sqrt, ConvertValue(Variance(sum, sumOfSquares, count, e), typeof(double))), typeof(double?))),
                NullConstant(typeof(double?)));

        return new AggregateFold(
            Expression.Lambda(Expression.New(typeof(VarState))),
            Expression.Lambda(accumulate, state, value),
            Expression.Lambda(resultBody, state));
    }

    // (SumOfSquares - e*(2*Sum - e*Count)) / (Count - 1), with e = Sum/Count supplied by the caller.
    private static Expression Variance(Expression sum, Expression sumOfSquares, Expression count, Expression e)
    {
        var twoSum = Binary(BinaryOperatorKind.Multiply, Expression.Constant(2m), sum);
        var eCount = Binary(BinaryOperatorKind.Multiply, e, count);
        var numerator = Binary(BinaryOperatorKind.Sub, sumOfSquares,
            Binary(BinaryOperatorKind.Multiply, e, Binary(BinaryOperatorKind.Sub, twoSum, eCount)));
        return Binary(BinaryOperatorKind.Divide, numerator, Binary(BinaryOperatorKind.Sub, count, Expression.Constant(1)));
    }

    // ---------------------------------------------------------------------
    // --- Operator / conversion helpers (the only NQuery-specific seam) ---
    // ---------------------------------------------------------------------

    private static Type ResolveBinaryType(BinaryOperatorKind op, Type left, Type right)
    {
        var result = BinaryOperator.Resolve(op, left, right);
        return result.Best?.Signature.ReturnType ?? throw new AggregateNotApplicableException();
    }

    private static Expression Binary(BinaryOperatorKind op, Expression left, Expression right)
    {
        var result = BinaryOperator.Resolve(op, left.Type, right.Type);
        var signature = result.Best?.Signature ?? throw new AggregateNotApplicableException();
        var l = ConvertValue(left, signature.GetParameterType(0));
        var r = ConvertValue(right, signature.GetParameterType(1));
        var method = signature.MethodInfo;

        return signature.Kind switch
        {
            BinaryOperatorKind.Add => Expression.Add(l, r, method),
            BinaryOperatorKind.Sub => Expression.Subtract(l, r, method),
            BinaryOperatorKind.Multiply => Expression.Multiply(l, r, method),
            BinaryOperatorKind.Divide => Expression.Divide(l, r, method),
            BinaryOperatorKind.Modulus => Expression.Modulo(l, r, method),
            BinaryOperatorKind.Less => Expression.LessThan(l, r, false, method),
            BinaryOperatorKind.LessOrEqual => Expression.LessThanOrEqual(l, r, false, method),
            BinaryOperatorKind.Greater => Expression.GreaterThan(l, r, false, method),
            BinaryOperatorKind.GreaterOrEqual => Expression.GreaterThanOrEqual(l, r, false, method),
            BinaryOperatorKind.Equal => Expression.Equal(l, r, false, method),
            BinaryOperatorKind.NotEqual => Expression.NotEqual(l, r, false, method),
            _ => throw new AggregateNotApplicableException()
        };
    }

    private static Expression ConvertValue(Expression value, Type target)
    {
        if (value.Type == target)
            return value;

        // Unbox/unwrap is a plain CLR conversion; NQuery's classifier is for value coercions.
        if (value.Type == typeof(object))
            return Expression.Convert(value, target);

        var conversion = Conversion.Classify(value.Type, target);
        if (!conversion.Exists)
            throw new AggregateNotApplicableException();

        var method = conversion.ConversionMethods is [var conversionMethod] ? conversionMethod : null;
        return Expression.Convert(value, target, method);
    }

    private static Expression Cast(Expression value, Type type)
    {
        return value.Type == type ? value : Expression.Convert(value, type);
    }

    private static Expression IsNull(Expression value)
    {
        if (Nullable.GetUnderlyingType(value.Type) is not null)
            return Expression.Not(Expression.Property(value, "HasValue"));

        if (!value.Type.IsValueType)
            return Expression.ReferenceEqual(value, Expression.Constant(null, value.Type));

        return Expression.Constant(false);
    }

    private static Expression NullConstant(Type type)
    {
        return Expression.Constant(null, type);
    }

    private static Type MakeNullable(Type type)
    {
        return type.IsValueType && Nullable.GetUnderlyingType(type) is null
            ? typeof(Nullable<>).MakeGenericType(type)
            : type;
    }

    private static bool IsNumeric(Type type)
    {
        return type == typeof(byte) || type == typeof(sbyte) ||
               type == typeof(short) || type == typeof(ushort) ||
               type == typeof(int) || type == typeof(uint) ||
               type == typeof(long) || type == typeof(ulong) ||
               type == typeof(decimal) || type == typeof(float) || type == typeof(double);
    }

    // ----------------------------------------------------
    // --- Accumulator state for the stateful aggregates ---
    // ----------------------------------------------------

    private struct AvgState<TSum>
        where TSum : struct
    {
        public AvgState(TSum? sum, int count)
        {
            Sum = sum;
            Count = count;
        }

        public TSum? Sum;
        public int Count;
    }

    private struct VarState
    {
        public VarState(decimal sum, decimal sumOfSquares, int count)
        {
            Sum = sum;
            SumOfSquares = sumOfSquares;
            Count = count;
        }

        public decimal Sum;
        public decimal SumOfSquares;
        public int Count;
    }
}
