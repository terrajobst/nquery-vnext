using System.Collections.Immutable;
using System.Linq.Expressions;

namespace NQuery.Metadata;

public abstract class FunctionDefinition
{
    private protected FunctionDefinition(string name, Type returnType, IEnumerable<ParameterDefinition> parameters)
    {
        ThrowIfNull(name);
        ThrowIfNull(returnType);
        ThrowIfNull(parameters);

        Name = name;
        ReturnType = returnType;
        Parameters = parameters.ToImmutableArray();
    }

    public string Name { get; }

    public Type ReturnType { get; }

    public ImmutableArray<ParameterDefinition> Parameters { get; }

    public IEnumerable<Type> GetParameterTypes()
    {
        return from p in Parameters
               select p.Type;
    }

    internal abstract Expression CreateInvocation(IEnumerable<Expression> arguments);

    public static FunctionDefinition Create(string name, Type returnType, IEnumerable<ParameterDefinition> parameters, Delegate function)
    {
        ThrowIfNull(function);

        return new DelegateFunctionDefinition(name, returnType, parameters, function);
    }

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
            return Expression.Call(instance, _function.Method, arguments);
        }
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
}
