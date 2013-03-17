using System;
using System.Linq.Expressions;

namespace NQuery.Symbols
{
    public abstract class PropertySymbol : Symbol
    {
        private readonly Type _type;

        public PropertySymbol(string name, Type type)
            : base(name)
        {
            _type = type;
        }

        public abstract Expression CreateInvocation(Expression instance);

        public override SymbolKind Kind
        {
            get { return SymbolKind.Property; }
        }

        public override Type Type
        {
            get { return _type; }
        }
    }
}