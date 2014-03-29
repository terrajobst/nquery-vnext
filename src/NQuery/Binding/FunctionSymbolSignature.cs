using System;

using NQuery.Symbols;

namespace NQuery.Binding
{
    internal sealed class FunctionSymbolSignature : Signature
    {
        private readonly FunctionSymbol _symbol;

        public FunctionSymbolSignature(FunctionSymbol symbol)
        {
            _symbol = symbol;
        }

        public override Type ReturnType
        {
            get { return _symbol.Type; }
        }

        public override Type GetParameterType(int index)
        {
            return _symbol.Parameters[index].Type;
        }

        public override int ParameterCount
        {
            get { return _symbol.Parameters.Length; }
        }

        public FunctionSymbol Symbol
        {
            get { return _symbol; }
        }

        public override string ToString()
        {
            return _symbol.ToString();
        }
    }
}