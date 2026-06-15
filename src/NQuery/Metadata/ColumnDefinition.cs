using System.Linq.Expressions;

namespace NQuery.Metadata;

public abstract class ColumnDefinition
{
    private protected ColumnDefinition(string name, Type dataType)
    {
        ThrowIfNull(name);
        ThrowIfNull(dataType);

        Name = name;
        DataType = dataType;
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
