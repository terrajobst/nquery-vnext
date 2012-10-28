using System;
using System.Collections.Generic;

namespace NQuery.Symbols
{
    public abstract class FunctionSymbol : InvocableSymbol
    {
        protected FunctionSymbol(string name, Type type, IList<ParameterSymbol> parameters)
            : base(name, type, parameters)
        {
        }

        protected FunctionSymbol(string name, Type type, params ParameterSymbol[] parameters)
            : this(name, type, (IList<ParameterSymbol>)parameters)
        {
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.Function; }
        }
    }
}