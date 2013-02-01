using System;

namespace NQuery.Symbols
{
    public sealed class BadVariableSymbol : VariableSymbol
    {
        public BadVariableSymbol(string name)
            : base(name, TypeFacts.Unknown)
        {
        }
    }
}