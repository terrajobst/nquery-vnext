using System.Linq.Expressions;
using System.Reflection;

using NQuery.CodeAnalysis.Iterators;

namespace NQuery.CodeAnalysis.Emit;

// Compiles a single output column into a Func<RowBuffer, T> that returns the value typed --
// never boxed through object (the box that RowBuffer.GetBoxedValue would otherwise cost). The
// column address is deterministic from the plan's output slots, so the delegate is compiled
// once and reused across executions and rows; that amortization is what makes avoiding the box
// worthwhile (a one-shot compile would cost more than the box it saves).
//
// SQL NULL maps to a caller-supplied value: a substitute constant (scalar/expression
// evaluation) or a throw (the typed reader, when T cannot represent null).
internal static class ColumnReaderCompiler
{
    private static readonly MethodInfo ReadObjectMethod = typeof(RowBuffer).GetMethod(nameof(RowBuffer.ReadObject))!;
    private static readonly MethodInfo Read32BitMethod = typeof(RowBuffer).GetMethod(nameof(RowBuffer.Read32Bit))!;
    private static readonly MethodInfo Read64BitMethod = typeof(RowBuffer).GetMethod(nameof(RowBuffer.Read64Bit))!;
    private static readonly MethodInfo Read128BitMethod = typeof(RowBuffer).GetMethod(nameof(RowBuffer.Read128Bit))!;

    // Substitutes whenNull for SQL NULL.
    public static Func<RowBuffer, T> Compile<T>(RowBufferColumn column, Type columnType, T whenNull)
    {
        return Compile<T>(column, columnType, Expression.Constant(whenNull, typeof(T)));
    }

    // Returns null for SQL NULL when T can represent it; otherwise throws (the typed reader
    // contract -- the caller must read a nullable T to observe NULL).
    public static Func<RowBuffer, T> CompileOrThrow<T>(RowBufferColumn column, Type columnType, int columnIndex)
    {
        Expression whenNull = typeof(T).CanBeNull()
            ? Expression.Constant(default(T), typeof(T))
            : Expression.Throw(
                Expression.New(
                    typeof(InvalidOperationException).GetConstructor(new[] { typeof(string) })!,
                    Expression.Constant($"Column {columnIndex} is NULL and cannot be read as non-nullable type '{typeof(T)}'. Read it as a nullable type.")),
                typeof(T));

        return Compile<T>(column, columnType, whenNull);
    }

    private static Func<RowBuffer, T> Compile<T>(RowBufferColumn column, Type columnType, Expression whenNull)
    {
        var rowBuffer = Expression.Parameter(typeof(RowBuffer), "rowBuffer");

        // A bare NULL column (the NullType sentinel) is always null, so it never reads bits.
        if (columnType.IsNull())
            return Expression.Lambda<Func<RowBuffer, T>>(whenNull, rowBuffer).Compile();

        // Read the column in its nullable shape (Nullable<T> for the value containers, a
        // possibly-null reference for the object container) -- what the typed Read* return.
        var index = Expression.Constant(column.Index);
        Expression read = column.Kind switch
        {
            RowBufferColumnKind.Object => Expression.Call(rowBuffer, ReadObjectMethod.MakeGenericMethod(columnType.GetNullableType()), index),
            RowBufferColumnKind.Bits32 => Expression.Call(rowBuffer, Read32BitMethod.MakeGenericMethod(columnType), index),
            RowBufferColumnKind.Bits64 => Expression.Call(rowBuffer, Read64BitMethod.MakeGenericMethod(columnType), index),
            RowBufferColumnKind.Bits128 => Expression.Call(rowBuffer, Read128BitMethod.MakeGenericMethod(columnType), index),
            _ => throw ExceptionBuilder.UnexpectedValue(column.Kind)
        };

        // Cache the read in a local so the null test and the value extraction don't read twice.
        var value = Expression.Variable(read.Type, "value");
        var assignment = Expression.Assign(value, read);

        Expression isNull;
        Expression raw;
        if (read.Type.IsNullableOfT())
        {
            isNull = Expression.Not(Expression.Property(value, nameof(Nullable<int>.HasValue)));
            raw = Expression.Call(value, nameof(Nullable<int>.GetValueOrDefault), Type.EmptyTypes);
        }
        else
        {
            isNull = Expression.ReferenceEqual(value, Expression.Constant(null, read.Type));
            raw = value;
        }

        var nonNull = ConvertTo<T>(raw, columnType);

        var body = Expression.Block(
            new[] { value },
            assignment,
            Expression.Condition(isNull, whenNull, nonNull));

        return Expression.Lambda<Func<RowBuffer, T>>(body, rowBuffer).Compile();
    }

    // Converts the column's value (its own non-nullable CLR type) to T. When T is reference-
    // compatible or the same value type, this is a direct, unboxed convert. When it is not
    // (e.g. ExecuteScalar<string>() over an int column), fall back to a box-and-cast so the
    // mismatch surfaces as an InvalidCastException at evaluation -- matching the previous
    // (T)(object)value behavior -- rather than failing while the reader is being built.
    private static Expression ConvertTo<T>(Expression value, Type columnType)
    {
        var directlyConvertible = typeof(T).IsAssignableFrom(columnType) || typeof(T).GetNonNullableType() == columnType;
        return directlyConvertible
            ? Expression.Convert(value, typeof(T))
            : Expression.Convert(Expression.Convert(value, typeof(object)), typeof(T));
    }
}
