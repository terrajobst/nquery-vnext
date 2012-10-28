using System;

using NQuery.Binding;

namespace NQuery.Symbols
{
    public sealed class BadVariableSymbol : VariableSymbol
    {
        public BadVariableSymbol(string name)
            : base(name, KnownTypes.Unknown)
        {
        }
    }
}