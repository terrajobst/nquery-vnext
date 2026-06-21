using System.Collections;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;

namespace NQuery.CodeAnalysis.Iterators;

// Compares two rows on a fixed list of sort keys. Compiled once per sort, so the column
// addresses, types, directions, and comparers are all baked into one delegate.
internal delegate int EmittedRowComparer(RowBuffer x, RowBuffer y);

// Compiles a multi-key row comparison into a single delegate, specialized to the sort's
// actual columns and comparers -- the same codegen approach the engine uses for scans,
// predicates, and computed values.
//
// Per column we read the value typed (Read32Bit<T>/.../ReadObject<T>, never boxed) and:
//   * with no registered comparer, call Comparer<T>.Default -- which the JIT devirtualizes
//     to the type's IComparable<T>/IComparable, so primitives compare without an interface
//     call or a box;
//   * with a registered comparer that implements IComparer<T>, call that typed Compare;
//   * otherwise fall back to the non-generic IComparer.Compare (the only path that boxes).
//
// NULLs order first regardless of direction (matching the row-at-a-time comparer this
// replaces); only the value comparison is negated for DESC. The first non-zero column
// result short-circuits.
internal static class EmittedRowComparerCompiler
{
    private static readonly MethodInfo ReadObjectMethod = typeof(RowBuffer).GetMethod(nameof(RowBuffer.ReadObject))!;
    private static readonly MethodInfo Read32BitMethod = typeof(RowBuffer).GetMethod(nameof(RowBuffer.Read32Bit))!;
    private static readonly MethodInfo Read64BitMethod = typeof(RowBuffer).GetMethod(nameof(RowBuffer.Read64Bit))!;
    private static readonly MethodInfo Read128BitMethod = typeof(RowBuffer).GetMethod(nameof(RowBuffer.Read128Bit))!;
    private static readonly MethodInfo NonGenericCompareMethod = typeof(IComparer).GetMethod(nameof(IComparer.Compare))!;

    public static EmittedRowComparer Compile(ImmutableArray<RowBufferColumn> columns, ImmutableArray<Type> types, ImmutableArray<IComparer> comparers)
    {
        var x = Expression.Parameter(typeof(RowBuffer), "x");
        var y = Expression.Parameter(typeof(RowBuffer), "y");

        var done = Expression.Label(typeof(int));
        var locals = new List<ParameterExpression>();
        var body = new List<Expression>();

        for (var i = 0; i < columns.Length; i++)
        {
            var (descending, inner) = Unwrap(comparers[i]);
            var type = types[i];

            // Read both sides once into locals (each Condition below reads them twice).
            var xValue = Expression.Variable(NullableShape(columns[i], type), $"x{i}");
            var yValue = Expression.Variable(NullableShape(columns[i], type), $"y{i}");
            locals.Add(xValue);
            locals.Add(yValue);
            body.Add(Expression.Assign(xValue, BuildRead(x, columns[i], type)));
            body.Add(Expression.Assign(yValue, BuildRead(y, columns[i], type)));

            var valueCompare = BuildValueCompare(inner, type, Lower(xValue), Lower(yValue));
            if (descending)
                valueCompare = Expression.Negate(valueCompare);

            // result = xNull ? (yNull ? 0 : -1)   // NULL sorts first, both directions
            //                : (yNull ? +1 : <value comparison>)
            var column = Expression.Condition(
                IsNull(xValue),
                Expression.Condition(IsNull(yValue), Expression.Constant(0), Expression.Constant(-1)),
                Expression.Condition(IsNull(yValue), Expression.Constant(1), valueCompare));

            var result = Expression.Variable(typeof(int), $"c{i}");
            locals.Add(result);
            body.Add(Expression.Assign(result, column));
            body.Add(Expression.IfThen(
                Expression.NotEqual(result, Expression.Constant(0)),
                Expression.Return(done, result)));
        }

        body.Add(Expression.Label(done, Expression.Constant(0)));

        var lambda = Expression.Lambda<EmittedRowComparer>(Expression.Block(locals, body), x, y);
        return lambda.Compile();
    }

    private static (bool Descending, IComparer Inner) Unwrap(IComparer comparer)
    {
        return comparer is NegatedComparer negated ? (true, negated.Inner) : (false, comparer);
    }

    // The shape a column reads back as: Nullable<T> for the bit-packed value kinds, the
    // (possibly null) reference for the object kind.
    private static Type NullableShape(RowBufferColumn column, Type type)
    {
        return column.Kind == RowBufferColumnKind.Object
                   ? type.GetNullableType()
                   : typeof(Nullable<>).MakeGenericType(type);
    }

    private static Expression BuildRead(ParameterExpression buffer, RowBufferColumn column, Type type)
    {
        var index = Expression.Constant(column.Index);
        return column.Kind switch
        {
            RowBufferColumnKind.Object => Expression.Call(buffer, ReadObjectMethod.MakeGenericMethod(type.GetNullableType()), index),
            RowBufferColumnKind.Bits32 => Expression.Call(buffer, Read32BitMethod.MakeGenericMethod(type), index),
            RowBufferColumnKind.Bits64 => Expression.Call(buffer, Read64BitMethod.MakeGenericMethod(type), index),
            RowBufferColumnKind.Bits128 => Expression.Call(buffer, Read128BitMethod.MakeGenericMethod(type), index),
            _ => throw ExceptionBuilder.UnexpectedValue(column.Kind)
        };
    }

    private static Expression IsNull(Expression value)
    {
        return value.Type.IsNullableOfT()
                   ? Expression.Not(Expression.Property(value, nameof(Nullable<int>.HasValue)))
                   : Expression.ReferenceEqual(value, Expression.Constant(null, value.Type));
    }

    // Drops the Nullable wrapper for the value kinds (guarded by the null checks above);
    // the object kind is already its own type.
    private static Expression Lower(Expression value)
    {
        return value.Type.IsNullableOfT()
                   ? Expression.Convert(value, value.Type.GetNonNullableType())
                   : value;
    }

    private static Expression BuildValueCompare(IComparer comparer, Type type, Expression xValue, Expression yValue)
    {
        // No registered comparer -> Comparer<T>.Default. The JIT devirtualizes its Compare
        // to the type's IComparable<T> (or IComparable), so this neither boxes nor makes a
        // virtual call for the primitives we bit-pack.
        if (ReferenceEquals(comparer, Comparer.Default))
        {
            var comparerType = typeof(Comparer<>).MakeGenericType(type);
            var defaultProperty = comparerType.GetProperty(nameof(Comparer<int>.Default))!;
            var compareMethod = comparerType.GetMethod(nameof(Comparer<int>.Compare), new[] { type, type })!;
            return Expression.Call(Expression.Property(null, defaultProperty), compareMethod, xValue, yValue);
        }

        // Registered comparer that is also typed -> call IComparer<T>.Compare, no boxing.
        var typedComparerType = typeof(IComparer<>).MakeGenericType(type);
        if (typedComparerType.IsInstanceOfType(comparer))
        {
            var compareMethod = typedComparerType.GetMethod(nameof(IComparer<int>.Compare))!;
            return Expression.Call(Expression.Constant(comparer, typedComparerType), compareMethod, xValue, yValue);
        }

        // Object-based registered comparer -> box the two operands and call IComparer.Compare.
        return Expression.Call(
            Expression.Constant(comparer, typeof(IComparer)),
            NonGenericCompareMethod,
            Expression.Convert(xValue, typeof(object)),
            Expression.Convert(yValue, typeof(object)));
    }
}
