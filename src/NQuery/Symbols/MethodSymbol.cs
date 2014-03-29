using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace NQuery.Symbols
{
    public abstract class MethodSymbol : InvocableSymbol
    {
        public MethodSymbol(string name, Type type, IEnumerable<ParameterSymbol> parameters)
            : base(name, type, parameters)
        {
        }

        public abstract Expression CreateInvocation(Expression instance, IEnumerable<Expression> arguments);

        public override SymbolKind Kind
        {
            get { return SymbolKind.Method; }
        }
    }
}