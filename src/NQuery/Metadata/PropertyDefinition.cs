using System.Linq.Expressions;
using System.Reflection;

using NQuery.CodeAnalysis;

namespace NQuery.Metadata;

public abstract class PropertyDefinition
{
    private protected PropertyDefinition(Type instanceType, string name, Type type)
    {
        ThrowIfNull(instanceType);
        ThrowIfNull(name);
        ThrowIfNull(type);

        // Nullable<T> is erased to T at the metadata boundary; nullability is tracked separately
        // by the engine (see ColumnDefinition for the rationale).
        InstanceType = instanceType.GetNonNullableType();
        Name = name;
        Type = type.GetNonNullableType();
    }

    // The type of the receiver the property is accessed on.
    public Type InstanceType { get; }

    public string Name { get; }

    public Type Type { get; }

    internal abstract Expression CreateInvocation(Expression instance);

    // The single lambda parameter is the instance the property is accessed on.
    public static PropertyDefinition Create<TInstance, TResult>(string name, System.Linq.Expressions.Expression<Func<TInstance, TResult>> expression)
    {
        ThrowIfNull(expression);

        return new ExpressionPropertyDefinition(name, expression.ReturnType, expression);
    }

    // The delegate's single parameter is the instance the property is accessed on; the property's
    // type is supplied explicitly (e.g. when it is only known at run time) and the accessor's
    // return value is converted to it.
    public static PropertyDefinition Create(string name, Type type, Delegate accessor)
    {
        ThrowIfNull(name);
        ThrowIfNull(type);
        ThrowIfNull(accessor);

        return new DelegatePropertyDefinition(name, type, accessor);
    }

    public static PropertyDefinition Create(string name, PropertyInfo property)
    {
        ThrowIfNull(name);
        ThrowIfNull(property);

        return new ReflectionPropertyDefinition(name, property);
    }

    public static PropertyDefinition Create(string name, FieldInfo field)
    {
        ThrowIfNull(name);
        ThrowIfNull(field);

        return new ReflectionFieldDefinition(name, field);
    }

    private sealed class ExpressionPropertyDefinition : PropertyDefinition
    {
        private readonly LambdaExpression _expression;

        public ExpressionPropertyDefinition(string name, Type type, LambdaExpression expression)
            : base(expression.Parameters[0].Type, name, type)
        {
            _expression = expression;
        }

        internal override Expression CreateInvocation(Expression instance)
        {
            var value = ExpressionInliner.Inline(_expression, new[] { instance });
            return value.Type == Type ? value : Expression.Convert(value, Type);
        }
    }

    private sealed class DelegatePropertyDefinition : PropertyDefinition
    {
        private readonly Delegate _accessor;

        public DelegatePropertyDefinition(string name, Type type, Delegate accessor)
            : base(accessor.Method.GetParameters()[0].ParameterType, name, type)
        {
            _accessor = accessor;
        }

        internal override Expression CreateInvocation(Expression instance)
        {
            var target = _accessor.Target is null ? null : Expression.Constant(_accessor.Target);

            // The delegate's single parameter is the instance; coerce it back to the delegate's
            // actual parameter type (see ReflectionMethodDefinition), then convert the result to the
            // declared property type.
            var value = Expression.Call(target, _accessor.Method, CoerceArguments.ToParameters(_accessor.Method, new[] { instance }));
            return value.Type == Type ? value : Expression.Convert(value, Type);
        }
    }

    private sealed class ReflectionPropertyDefinition : PropertyDefinition
    {
        public ReflectionPropertyDefinition(string name, PropertyInfo propertyInfo)
            : base(propertyInfo.DeclaringType!, name, propertyInfo.PropertyType)
        {
            PropertyInfo = propertyInfo;
        }

        public PropertyInfo PropertyInfo { get; }

        internal override Expression CreateInvocation(Expression instance)
        {
            return Expression.MakeMemberAccess(instance, PropertyInfo);
        }
    }

    private sealed class ReflectionFieldDefinition : PropertyDefinition
    {
        public ReflectionFieldDefinition(string name, FieldInfo fieldInfo)
            : base(fieldInfo.DeclaringType!, name, fieldInfo.FieldType)
        {
            FieldInfo = fieldInfo;
        }

        public FieldInfo FieldInfo { get; }

        internal override Expression CreateInvocation(Expression instance)
        {
            return Expression.MakeMemberAccess(instance, FieldInfo);
        }
    }
}
