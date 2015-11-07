using System;

using NQuery.Symbols;

namespace NQuery.Binding
{
    internal sealed class FunctionSymbolSignature : Signature
    {
        public FunctionSymbolSignature(FunctionSymbol symbol)
        {
            Symbol = symbol;
        }

        public override Type ReturnType
        {
            get { return Symbol.Type; }
        }

        public override Type GetParameterType(int index)
        {
            return Symbol.Parameters[index].Type;
        }

        public override int ParameterCount
        {
            get { return Symbol.Parameters.Length; }
        }

        public FunctionSymbol Symbol { get; }

        public override string ToString()
        {
            return Symbol.ToString();
        }
    }
}