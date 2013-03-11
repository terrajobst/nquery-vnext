using System;

namespace NQuery.Symbols
{
    public class ColumnSymbol : Symbol
    {
        private readonly Type _type;

        internal ColumnSymbol(string name, Type type)
            : base(name)
        {
            _type = type;
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.Column; }
        }

        public override Type Type
        {
            get { return _type; }
        }
    }
}