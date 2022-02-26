using System.Linq.Expressions;

namespace NQuery.Symbols
{
    public abstract class FunctionSymbol : InvocableSymbol
    {
        protected FunctionSymbol(string name, Type type, IEnumerable<ParameterSymbol> parameters)
            : base(name, type, parameters)
        {
        }

        protected FunctionSymbol(string name, Type type, params ParameterSymbol[] parameters)
            : this(name, type, (IEnumerable<ParameterSymbol>)parameters)
        {
        }

        public abstract Expression CreateInvocation(IEnumerable<Expression> arguments);

        public override SymbolKind Kind
        {
            get { return SymbolKind.Function; }
        }
    }

    public sealed class FunctionSymbol<TResult> : FunctionSymbol
    {
        public FunctionSymbol(string name, Func<TResult> function)
            : base(name, typeof(TResult))
        {
            Function = function;
        }

        public override Expression CreateInvocation(IEnumerable<Expression> arguments)
        {
            return Expression.Call(Function.Method, arguments);
        }

        public Func<TResult> Function { get; }
    }

    public sealed class FunctionSymbol<T, TResult> : FunctionSymbol
    {
        public FunctionSymbol(string name, Func<T, TResult> function)
            : this(name, @"arg", function)
        {

        }

        public FunctionSymbol(string name, string argumentName, Func<T, TResult> function)
            : base(name, typeof(TResult), new ParameterSymbol(argumentName, typeof(T)))
        {
            Function = function;
        }

        public override Expression CreateInvocation(IEnumerable<Expression> arguments)
        {
            return Expression.Call(Function.Method, arguments);
        }

        public Func<T, TResult> Function { get; }
    }

    public sealed class FunctionSymbol<T1, T2, TResult> : FunctionSymbol
    {
        public FunctionSymbol(string name, Func<T1, T2, TResult> function)
            : this(name, @"arg1", @"arg2", function)
        {
        }

        public FunctionSymbol(string name, string parameterName1, string parameterName2, Func<T1, T2, TResult> function)
            : base(name, typeof(TResult), new ParameterSymbol(parameterName1, typeof(T1)), new ParameterSymbol(parameterName2, typeof(T2)))
        {
            Function = function;
        }

        public override Expression CreateInvocation(IEnumerable<Expression> arguments)
        {
            return Expression.Call(Function.Method, arguments);
        }

        public Func<T1, T2, TResult> Function { get; }
    }

    public sealed class FunctionSymbol<T1, T2, T3, TResult> : FunctionSymbol
    {
        public FunctionSymbol(string name, Func<T1, T2, T3, TResult> function)
            : this(name, @"arg1", @"arg2", @"arg3", function)
        {
        }

        public FunctionSymbol(string name, string parameterName1, string parameterName2, string parameterName3, Func<T1, T2, T3, TResult> function)
            : base(name, typeof(TResult), new ParameterSymbol(parameterName1, typeof(T1)), new ParameterSymbol(parameterName2, typeof(T2)), new ParameterSymbol(parameterName3, typeof(T3)))
        {
            Function = function;
        }

        public override Expression CreateInvocation(IEnumerable<Expression> arguments)
        {
            return Expression.Call(Function.Method, arguments);
        }

        public Func<T1, T2, T3, TResult> Function { get; }
    }
}
