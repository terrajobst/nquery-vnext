using System;

namespace NQuery.Symbols
{
    public abstract class ColumnSymbol : Symbol
    {
        internal ColumnSymbol(string name, Type type)
            : base(name)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            Type = type;
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.Column; }
        }

        public override Type Type { get; }
    }
}