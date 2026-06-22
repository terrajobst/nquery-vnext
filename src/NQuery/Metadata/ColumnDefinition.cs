using System.Linq.Expressions;

using NQuery.CodeAnalysis;

namespace NQuery.Metadata;

public abstract class ColumnDefinition
{
    private protected ColumnDefinition(string name, Type dataType)
    {
        ThrowIfNull(name);
        ThrowIfNull(dataType);

        // The engine models nullability separately from the CLR type (SQL NULL is orthogonal,
        // and the row buffer tracks null apart from the stored bits), so Nullable<T> is erased
        // to T at the metadata boundary -- value types stay unboxed and the type system only
        // ever sees non-nullable CLR types. See TypeFacts.GetNonNullableType.
        Name = name;
        DataType = dataType.GetNonNullableType();
    }

    public string Name { get; }

    public Type DataType { get; }

    internal abstract Expression CreateInvocation(Expression instance);

    // The single lambda parameter is the row the column value is computed from.
    public static ColumnDefinition Create<TRow, TValue>(string name, System.Linq.Expressions.Expression<Func<TRow, TValue>> expression)
    {
        ThrowIfNull(name);
        ThrowIfNull(expression);

        return new ExpressionColumnDefinition(name, expression.ReturnType, expression);
    }

    // Same, but with the column's data type supplied explicitly (e.g. when it is only known at
    // run time); the accessor produces the boxed value.
    public static ColumnDefinition Create<TRow>(string name, Type type, System.Linq.Expressions.Expression<Func<TRow, object>> expression)
    {
        ThrowIfNull(name);
        ThrowIfNull(type);
        ThrowIfNull(expression);

        return new ExpressionColumnDefinition(name, type, expression);
    }
}
