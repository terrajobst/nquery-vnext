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
        ThrowIfNull(expression);

        return new ExpressionColumnDefinition(name, expression.ReturnType, expression);
    }

    // Non-generic counterpart of the strongly-typed overload, for callers that build the accessor
    // as an expression tree (no generic type parameters). The single lambda parameter is the row;
    // the value is inlined and the column's data type is supplied explicitly.
    public static ColumnDefinition Create(string name, Type type, LambdaExpression expression)
    {
        ThrowIfNull(name);
        ThrowIfNull(type);
        ThrowIfNull(expression);

        return new ExpressionColumnDefinition(name, type, expression);
    }

    // The delegate's single parameter is the row the column value is computed from; the column's
    // data type is supplied explicitly (e.g. when it is only known at run time).
    public static ColumnDefinition Create(string name, Type type, Delegate accessor)
    {
        ThrowIfNull(name);
        ThrowIfNull(type);
        ThrowIfNull(accessor);

        return new DelegateColumnDefinition(name, type, accessor);
    }

    // Builds a column that reads a table-row property; the row instance is coerced to the row
    // type before the property accessor runs.
    internal static ColumnDefinition Create(Type rowType, PropertyDefinition property)
    {
        ThrowIfNull(rowType);
        ThrowIfNull(property);

        return new PropertyColumnDefinition(rowType, property);
    }

    private sealed class ExpressionColumnDefinition : ColumnDefinition
    {
        private readonly LambdaExpression _expression;

        public ExpressionColumnDefinition(string name, Type dataType, LambdaExpression expression)
            : base(name, dataType)
        {
            _expression = expression;
        }

        // Returns the accessor's value in its own CLR type, so a typed accessor
        // (Func<TRow, TValue>) avoids boxing. The row writer converts it into the typed slot.
        internal override Expression CreateInvocation(Expression instance)
        {
            return ExpressionInliner.Inline(_expression, new[] { instance });
        }
    }

    private sealed class DelegateColumnDefinition : ColumnDefinition
    {
        private readonly Delegate _accessor;

        public DelegateColumnDefinition(string name, Type dataType, Delegate accessor)
            : base(name, dataType)
        {
            _accessor = accessor;
        }

        // Returns the accessor's value in its own CLR type; the row writer converts it into the
        // typed slot (an object-returning accessor, e.g. a DataRow indexer, stays boxed until then).
        internal override Expression CreateInvocation(Expression instance)
        {
            var target = _accessor.Target is null ? null : Expression.Constant(_accessor.Target);

            // The delegate's single parameter is the row; coerce the object-typed row expression to
            // the delegate's actual parameter type (see ReflectionMethodDefinition).
            return Expression.Call(target, _accessor.Method, CoerceArguments.ToParameters(_accessor.Method, new[] { instance }));
        }
    }

    private sealed class PropertyColumnDefinition : ColumnDefinition
    {
        private readonly Type _rowType;
        private readonly PropertyDefinition _property;

        public PropertyColumnDefinition(Type rowType, PropertyDefinition property)
            : base(property.Name, property.Type)
        {
            _rowType = rowType;
            _property = property;
        }

        // Returns the property value in its own CLR type -- not boxed to object. The row
        // writer lifts it to the column's nullable shape and stores it typed, so a value-typed
        // column never boxes on the way into the row buffer.
        internal override Expression CreateInvocation(Expression instance)
        {
            return _property.CreateInvocation(Expression.Convert(instance, _rowType));
        }
    }
}
