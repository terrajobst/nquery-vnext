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

    internal Expression CreateInvocation(IEnumerable<Expression> arguments)
    {
        var function = FunctionDelegate;
        var instance = function.Target == null ? null : Expression.Constant(function.Target);
        return Expression.Call(instance, function.Method, arguments);
    }

    protected abstract Delegate FunctionDelegate { get; }

    public static FunctionDefinition Create(string name, Type returnType, IEnumerable<ParameterDefinition> parameters, Delegate function)
    {
        ThrowIfNull(function);

        return new DelegateFunctionDefinition(name, returnType, parameters, function);
    }

    public static FunctionDefinition Create<TResult>(string name, Func<TResult> function)
    {
        return Create(name, typeof(TResult), [], function);
    }

    public static FunctionDefinition Create<T, TResult>(string name, Func<T, TResult> function)
    {
        return Create(name, @"arg", function);
    }

    public static FunctionDefinition Create<T, TResult>(string name, string parameterName, Func<T, TResult> function)
    {
        var parameters = new[] { 
            ParameterDefinition.Create(parameterName, typeof(T))
        };
        return Create(name, typeof(TResult), parameters, function);
    }

    public static FunctionDefinition Create<T1, T2, TResult>(string name, Func<T1, T2, TResult> function)
    {
        return Create(name, @"arg1", @"arg2", function);
    }

    public static FunctionDefinition Create<T1, T2, TResult>(string name, string parameterName1, string parameterName2, Func<T1, T2, TResult> function)
    {
        var parameters = new[] {
            ParameterDefinition.Create(parameterName1, typeof(T1)),
            ParameterDefinition.Create(parameterName2, typeof(T2))
        };
        return Create(name, typeof(TResult), parameters, function);
    }

    public static FunctionDefinition Create<T1, T2, T3, TResult>(string name, Func<T1, T2, T3, TResult> function)
    {
        return Create(name, @"arg1", @"arg2", @"arg3", function);
    }

    public static FunctionDefinition Create<T1, T2, T3, TResult>(string name, string parameterName1, string parameterName2, string parameterName3, Func<T1, T2, T3, TResult> function)
    {
        var parameters = new[] { 
            ParameterDefinition.Create(parameterName1, typeof(T1)),
            ParameterDefinition.Create(parameterName2, typeof(T2)),
            ParameterDefinition.Create(parameterName3, typeof(T3))
        };
        return Create(name, typeof(TResult), parameters, function);
    }

    private sealed class DelegateFunctionDefinition : FunctionDefinition
    {
        private readonly Delegate _function;

        public DelegateFunctionDefinition(string name, Type returnType, IEnumerable<ParameterDefinition> parameters, Delegate function)
            : base(name, returnType, parameters)
        {
            _function = function;
        }

        protected override Delegate FunctionDelegate
        {
            get { return _function; }
        }
    }
}
