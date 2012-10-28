using System;
using System.Collections.Generic;

namespace NQuery.Language.Symbols
{
    public class MethodSymbol : InvocableSymbol
    {
        public MethodSymbol(string name, Type type, IList<ParameterSymbol> parameters)
            : base(name, type, parameters)
        {
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.Method; }
        }
    }
}