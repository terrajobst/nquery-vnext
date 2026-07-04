using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;

using NQuery.CodeAnalysis;

namespace NQuery.Metadata;

public abstract class FunctionDefinition
{
    private protected FunctionDefinition(string name, Type returnType, IEnumerable<ParameterDefinition> parameters)
    {
        ThrowIfNull(name);
        ThrowIfNull(returnType);
        ThrowIfNull(parameters);

        // Nullable<T> is erased to T at the metadata boundary; nullability is tracked separately
        // by the engine (see ColumnDefinition). Parameters self-erase via ParameterDefinition.
        Name = name;
        ReturnType = returnType.GetNonNullableType();
        Parameters = [.. parameters];
        SignatureHashCode = SignatureEqualityComparer.GetHashCode(this);
    }

    public string Name { get; }

    public Type ReturnType { get; }

    public ImmutableArray<ParameterDefinition> Parameters { get; }

    // Precomputed signature hash (name + parameter types); see SignatureEqualityComparer.
    internal int SignatureHashCode { get; }

    internal abstract Expression CreateInvocation(IEnumerable<Expression> arguments);

    public static FunctionDefinition Create<TResult>(string name, System.Linq.Expressions.Expression<Func<TResult>> expression)
    {
        ThrowIfNull(expression);

        return new ExpressionFunctionDefinition(name, expression);
    }

    public static FunctionDefinition Create<T, TResult>(string name, System.Linq.Expressions.Expression<Func<T, TResult>> expression)
    {
        ThrowIfNull(expression);

        return new ExpressionFunctionDefinition(name, expression);
    }

    public static FunctionDefinition Create<T1, T2, TResult>(string name, System.Linq.Expressions.Expression<Func<T1, T2, TResult>> expression)
    {
        ThrowIfNull(expression);

        return new ExpressionFunctionDefinition(name, expression);
    }

    public static FunctionDefinition Create<T1, T2, T3, TResult>(string name, System.Linq.Expressions.Expression<Func<T1, T2, T3, TResult>> expression)
    {
        ThrowIfNull(expression);

        return new ExpressionFunctionDefinition(name, expression);
    }

    public static FunctionDefinition Create(string name, Type returnType, IEnumerable<ParameterDefinition> parameters, Delegate function)
    {
        ThrowIfNull(function);

        return new DelegateFunctionDefinition(name, returnType, parameters, function);
    }

    public static FunctionDefinition Create(string name, MethodInfo method, object? instance = null)
    {
        ThrowIfNull(name);
        ThrowIfNull(method);

        // A function has no query-level receiver. Without a bound instance the method must be static;
        // with one it must be an instance method whose declaring type accepts that instance.
        if (instance is null)
        {
            if (!method.IsStatic)
                throw new ArgumentException(Resources.FunctionMethodMustBeStatic, nameof(method));
        }
        else
        {
            if (method.IsStatic)
                throw new ArgumentException(Resources.FunctionMethodMustBeInstance, nameof(method));

            if (!method.DeclaringType!.IsInstanceOfType(instance))
            {
                var message = string.Format(Resources.FunctionInstanceTypeMismatch, instance.GetType(), method.DeclaringType);
                throw new ArgumentException(message, nameof(instance));
            }
        }

        return new ReflectionFunctionDefinition(name, method, instance);
    }

    private sealed class ExpressionFunctionDefinition : FunctionDefinition
    {
        private readonly LambdaExpression _expression;

        public ExpressionFunctionDefinition(string name, LambdaExpression expression)
            : base(name, expression.ReturnType, GetParameters(expression))
        {
            _expression = expression;
        }

        private static IEnumerable<ParameterDefinition> GetParameters(LambdaExpression expression)
        {
            return expression.Parameters.Select(p => ParameterDefinition.Create(p.Name!, p.Type));
        }

        internal override Expression CreateInvocation(IEnumerable<Expression> arguments)
        {
            return ExpressionInliner.Inline(_expression, arguments);
        }
    }

    private sealed class DelegateFunctionDefinition : FunctionDefinition
    {
        private readonly Delegate _function;

        public DelegateFunctionDefinition(string name, Type returnType, IEnumerable<ParameterDefinition> parameters, Delegate function)
            : base(name, returnType, parameters)
        {
            _function = function;
        }

        internal override Expression CreateInvocation(IEnumerable<Expression> arguments)
        {
            var instance = _function.Target is null ? null : Expression.Constant(_function.Target);
            // See ReflectionMethodDefinition: coerce lowered arguments back to the delegate's
            // actual parameter types, which may still be Nullable<T>.
            return Expression.Call(instance, _function.Method, CoerceArguments.ToParameters(_function.Method, arguments));
        }
    }

    private sealed class ReflectionFunctionDefinition : FunctionDefinition
    {
        private readonly MethodInfo _methodInfo;
        private readonly object? _instance;

        public ReflectionFunctionDefinition(string name, MethodInfo methodInfo, object? instance)
            : base(name, methodInfo.ReturnType, ConvertParameters(methodInfo))
        {
            _methodInfo = methodInfo;
            _instance = instance;
        }

        private static IEnumerable<ParameterDefinition> ConvertParameters(MethodInfo methodInfo)
        {
            return methodInfo.GetParameters()
                             .Select(p => ParameterDefinition.Create(p.Name!, p.ParameterType));
        }

        internal override Expression CreateInvocation(IEnumerable<Expression> arguments)
        {
            // Static when no instance is bound; otherwise the call closes over the receiver (see
            // FunctionDefinition.Create for the static/instance validation). Coerce the lowered
            // arguments back to the method's actual parameter types, which may still be Nullable<T>
            // (see ReflectionMethodDefinition).
            var instance = _instance is null ? null : Expression.Constant(_instance, _methodInfo.DeclaringType!);
            return Expression.Call(instance, _methodInfo, CoerceArguments.ToParameters(_methodInfo, arguments));
        }
    }
}
