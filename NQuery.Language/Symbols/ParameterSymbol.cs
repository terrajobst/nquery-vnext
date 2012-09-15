using System;
using NQuery.Language.Binding;

namespace NQuery.Language.Symbols
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

        public override string ToString()
        {
            return string.Format("{0} AS {1}", Name, Type.ToDisplayName());
        }
    }
}