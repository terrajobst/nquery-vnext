using System;
using NQuery.Language.Symbols;

namespace NQuery.Language.Runtime
{
    public sealed class FunctionSymbol<T,TResult> : FunctionSymbol
    {
        private readonly Func<T, TResult> _function;

        public FunctionSymbol(string name, Func<T,TResult> function)
            : this(name, "arg", function)
        {
                    
        }

        public FunctionSymbol(string name, string argumentName, Func<T,TResult> function)
            : base(name, typeof(TResult), new ParameterSymbol(argumentName, typeof(T)))
        {
            _function = function;
        }

        public Func<T, TResult> Function
        {
            get { return _function; }
        }
    }

    public sealed class FunctionSymbol<T1, T2, TResult> : FunctionSymbol
    {
        private readonly Func<T1, T2, TResult> _function;

        public FunctionSymbol(string name, Func<T1, T2, TResult> function)
            : this(name, "arg1", "arg2", function)
        {
        }

        public FunctionSymbol(string name, string parameterName1, string parameterName2, Func<T1,T2, TResult> function)
            : base(name, typeof(TResult), new ParameterSymbol(parameterName1, typeof(T1)), new ParameterSymbol(parameterName2, typeof(T2)))
        {
            _function = function;
        }

        public Func<T1, T2, TResult> Function
        {
            get { return _function; }
        }
    }

    public sealed class FunctionSymbol<T1, T2, T3, TResult> : FunctionSymbol
    {
        private readonly Func<T1, T2, T3, TResult> _function;

        public FunctionSymbol(string name, Func<T1, T2, T3, TResult> function)
            : this(name, "arg1", "arg2", "arg3", function)
        {
        }

        public FunctionSymbol(string name, string parameterName1, string parameterName2, string parameterName3, Func<T1, T2, T3, TResult> function)
            : base(name, typeof(TResult), new ParameterSymbol(parameterName1, typeof(T1)), new ParameterSymbol(parameterName2, typeof(T2)), new ParameterSymbol(parameterName3, typeof(T2)))
        {
            _function = function;
        }

        public Func<T1, T2, T3, TResult> Function
        {
            get { return _function; }
        }
    }

}