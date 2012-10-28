using System;

namespace NQuery.Language.Symbols
{
    public class VariableSymbol : Symbol
    {
        private readonly Type _type;

        public VariableSymbol(string name, Type type)
            : base(name)
        {
            _type = type;
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.Variable; }
        }

        public override Type Type
        {
            get { return _type; }
        }
    }
}