using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;

using NQuery.CodeAnalysis;

namespace NQuery.Metadata;

public abstract class MethodDefinition
{
    private protected MethodDefinition(Type instanceType, string name, Type returnType, IEnumerable<ParameterDefinition> parameters)
    {
        ThrowIfNull(instanceType);
        ThrowIfNull(name);
        ThrowIfNull(returnType);
        ThrowIfNull(parameters);

        // Nullable<T> is erased to T at the metadata boundary; nullability is tracked separately
        // by the engine (see ColumnDefinition). Parameters self-erase via ParameterDefinition.
        InstanceType = instanceType.GetNonNullableType();
        Name = name;
        ReturnType = returnType.GetNonNullableType();
        Parameters = [.. parameters];
    }

    // The type of the receiver the method is invoked on (its first, implicit argument).
    public Type InstanceType { get; }

    public string Name { get; }

    public Type ReturnType { get; }

    public ImmutableArray<ParameterDefinition> Parameters { get; }

    internal abstract Expression CreateInvocation(Expression instance, IEnumerable<Expression> arguments);

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

    // The delegate's first parameter is the instance the method is invoked on; the remaining
    // parameters correspond to `parameters` (the method's arguments).
    public static MethodDefinition Create(string name, Type returnType, IEnumerable<ParameterDefinition> parameters, Delegate method)
    {
        ThrowIfNull(name);
        ThrowIfNull(returnType);
        ThrowIfNull(parameters);
        ThrowIfNull(method);

        return new DelegateMethodDefinition(name, returnType, parameters, method);
    }

    public static MethodDefinition Create(string name, MethodInfo method)
    {
        ThrowIfNull(name);
        ThrowIfNull(method);

        return new ReflectionMethodDefinition(name, method);
    }

    private sealed class ExpressionMethodDefinition : MethodDefinition
    {
        private readonly LambdaExpression _expression;

        public ExpressionMethodDefinition(string name, LambdaExpression expression)
            : base(expression.Parameters[0].Type, name, expression.ReturnType, GetParameters(expression))
        {
            _expression = expression;
        }

        private static IEnumerable<ParameterDefinition> GetParameters(LambdaExpression expression)
        {
            // The first lambda parameter is the instance; the rest are the method's parameters.
            return expression.Parameters.Skip(1).Select(p => ParameterDefinition.Create(p.Name!, p.Type));
        }

        internal override Expression CreateInvocation(Expression instance, IEnumerable<Expression> arguments)
        {
            return ExpressionInliner.Inline(_expression, new[] { instance }.Concat(arguments));
        }
    }

    private sealed class DelegateMethodDefinition : MethodDefinition
    {
        private readonly Delegate _method;

        public DelegateMethodDefinition(string name, Type returnType, IEnumerable<ParameterDefinition> parameters, Delegate method)
            : base(method.Method.GetParameters()[0].ParameterType, name, returnType, parameters)
        {
            _method = method;
        }

        internal override Expression CreateInvocation(Expression instance, IEnumerable<Expression> arguments)
        {
            var target = _method.Target is null ? null : Expression.Constant(_method.Target);

            // The delegate's first parameter is the instance the method is invoked on; the rest are
            // the method's arguments. Coerce the lowered arguments back to the delegate's actual
            // parameter types, which may still be Nullable<T> (see ReflectionMethodDefinition).
            var allArguments = new[] { instance }.Concat(arguments);
            return Expression.Call(target, _method.Method, CoerceArguments.ToParameters(_method.Method, allArguments));
        }
    }

    // Kept internal (not private) so tests can assert a definition is reflection-backed and inspect
    // its MethodInfo; see ReflectionProviderTests.
    internal sealed class ReflectionMethodDefinition : MethodDefinition
    {
        public ReflectionMethodDefinition(string name, MethodInfo methodInfo)
            : base(methodInfo.DeclaringType!, name, methodInfo.ReturnType, ConvertParameters(methodInfo))
        {
            MethodInfo = methodInfo;
        }

        public MethodInfo MethodInfo { get; }

        private static IEnumerable<ParameterDefinition> ConvertParameters(MethodInfo methodInfo)
        {
            return methodInfo.GetParameters()
                             .Select(p => ParameterDefinition.Create(p.Name!, p.ParameterType));
        }

        internal override Expression CreateInvocation(Expression instance, IEnumerable<Expression> arguments)
        {
            // The declared parameter types are erased (Nullable<T> -> T), and the engine lowers
            // arguments to their non-nullable form before calling, so a lowered `int` may arrive for
            // a real `int?` parameter. Expression.Call requires exact assignability, so coerce each
            // argument back to the method's actual parameter type.
            return Expression.Call(instance, MethodInfo, CoerceArguments.ToParameters(MethodInfo, arguments));
        }
    }
}
