using System;

namespace NQuery.Symbols
{
    public class ParameterSymbol : Symbol
    {
        private readonly Type _type;

        public ParameterSymbol(string name, Type type)
            : base(name)
        {
            _type = type;
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.Parameter; }
        }

        public override Type Type
        {
            get { return _type; }
        }
    }
}