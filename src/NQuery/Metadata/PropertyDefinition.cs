using System.Linq.Expressions;
using System.Reflection;

namespace NQuery.Metadata;

public abstract class PropertyDefinition
{
    private protected PropertyDefinition(string name, Type type)
    {
        ThrowIfNull(name);
        ThrowIfNull(type);

        Name = name;
        Type = type;
    }

    public string Name { get; }

    public Type Type { get; }

    internal abstract Expression CreateInvocation(Expression instance);

    public static PropertyDefinition Create(PropertyInfo propertyInfo)
    {
        ThrowIfNull(propertyInfo);

        return Create(propertyInfo, propertyInfo.Name);
    }

    public static PropertyDefinition Create(PropertyInfo propertyInfo, string name)
    {
        ThrowIfNull(propertyInfo);
        ThrowIfNull(name);

        return new ReflectionPropertyDefinition(propertyInfo, name);
    }

    public static PropertyDefinition Create(FieldInfo fieldInfo)
    {
        ThrowIfNull(fieldInfo);

        return Create(fieldInfo, fieldInfo.Name);
    }

    public static PropertyDefinition Create(FieldInfo fieldInfo, string name)
    {
        ThrowIfNull(fieldInfo);
        ThrowIfNull(name);

        return new ReflectionFieldDefinition(fieldInfo, name);
    }

    // The single lambda parameter is the instance the property is accessed on.

    public static PropertyDefinition Create<TInstance, TResult>(string name, System.Linq.Expressions.Expression<Func<TInstance, TResult>> expression)
    {
        ThrowIfNull(name);
        ThrowIfNull(expression);

        return new ExpressionPropertyDefinition(name, expression.ReturnType, expression);
    }

    // Same, but with the property's type supplied explicitly (e.g. when it is only known at
    // run time); the accessor produces the boxed value.
    public static PropertyDefinition Create<TInstance>(string name, Type type, System.Linq.Expressions.Expression<Func<TInstance, object>> expression)
    {
        ThrowIfNull(name);
        ThrowIfNull(type);
        ThrowIfNull(expression);

        return new ExpressionPropertyDefinition(name, type, expression);
    }
}
