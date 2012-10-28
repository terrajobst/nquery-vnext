using System;

using NQuery.Symbols;

namespace NQuery.Binding
{
    internal sealed class MethodSymbolSignature : Signature
    {
        private readonly MethodSymbol _symbol;

        public MethodSymbolSignature(MethodSymbol symbol)
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
            get { return _symbol.Parameters.Count; }
        }

        public MethodSymbol Symbol
        {
            get { return _symbol; }
        }

        public override string ToString()
        {
            return _symbol.ToString();
        }
    }
}