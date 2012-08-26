using System;
using System.Collections.Generic;

namespace NQuery.Language.Symbols
{
    public sealed class FunctionSymbol : InvocableSymbol
    {
        public FunctionSymbol(string name, Type type, IList<ParameterSymbol> parameters)
            : base(name, type, parameters)
        {
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.Function; }
        }
    }
}