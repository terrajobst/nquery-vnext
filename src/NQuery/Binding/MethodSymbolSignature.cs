using NQuery.Symbols;

namespace NQuery.Binding
{
    internal sealed class MethodSymbolSignature : Signature
    {
        public MethodSymbolSignature(MethodSymbol symbol)
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

        public MethodSymbol Symbol { get; }

        public override string ToString()
        {
            return Symbol.ToString();
        }
    }
}