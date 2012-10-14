using System;
using NQuery.Language.Binding;

namespace NQuery.Language.Symbols
{
    public sealed class BadSymbol : Symbol
    {
        private readonly Type _type;

        public BadSymbol(string name)
            : this(name, KnownTypes.Unknown)
        {
        }

        public BadSymbol(string name, Type type)
            : base(name)
        {
            _type = type;
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.BadSymbol; }
        }

        public override Type Type
        {
            get { return _type; }
        }
    }
}