using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;

using NQuery.CodeAnalysis;

namespace NQuery.Metadata;

public abstract class MethodDefinition
{
    private protected MethodDefinition(string name, Type returnType, IEnumerable<ParameterDefinition> parameters)
    {
        ThrowIfNull(name);
        ThrowIfNull(returnType);
        ThrowIfNull(parameters);

        // Nullable<T> is erased to T at the metadata boundary; nullability is tracked separately
        // by the engine (see ColumnDefinition). Parameters self-erase via ParameterDefinition.
        Name = name;
        ReturnType = returnType.GetNonNullableType();
        Parameters = [.. parameters];
    }

    public string Name { get; }

    public Type ReturnType { get; }

    public ImmutableArray<ParameterDefinition> Parameters { get; }

    public IEnumerable<Type> GetParameterTypes()
    {
        return from p in Parameters
               select p.Type;
    }

    internal abstract Expression CreateInvocation(Expression instance, IEnumerable<Expression> arguments);

    public static MethodDefinition Create(MethodInfo methodInfo)
    {
        ThrowIfNull(methodInfo);

        return Create(methodInfo, methodInfo.Name);
    }

    public static MethodDefinition Create(MethodInfo methodInfo, string name)
    {
        ThrowIfNull(methodInfo);
        ThrowIfNull(name);

        return new ReflectionMethodDefinition(methodInfo, name);
    }

    // The first lambda parameter is the instance the method is invoked on; the remaining
    // parameters are the method's arguments.

    public static MethodDefinition Create<TInstance, TResult>(string name, System.Linq.Expressions.Expression<Func<TInstance, TResult>> expression)
    {
        ThrowIfNull(expression);

        return new ExpressionMethodDefinition(name, expression);
    }

    public static MethodDefinition Create<TInstance, T1, TResult>(string name, System.Linq.Expressions.Expression<Func<TInstance, T1, TResult>> expression)
    {
        ThrowIfNull(expression);

        return new ExpressionMethodDefinition(name, expression);
    }

    public static MethodDefinition Create<TInstance, T1, T2, TResult>(string name, System.Linq.Expressions.Expression<Func<TInstance, T1, T2, TResult>> expression)
    {
        ThrowIfNull(expression);

        return new ExpressionMethodDefinition(name, expression);
    }

    public static MethodDefinition Create<TInstance, T1, T2, T3, TResult>(string name, System.Linq.Expressions.Expression<Func<TInstance, T1, T2, T3, TResult>> expression)
    {
        ThrowIfNull(expression);

        return new ExpressionMethodDefinition(name, expression);
    }
}
