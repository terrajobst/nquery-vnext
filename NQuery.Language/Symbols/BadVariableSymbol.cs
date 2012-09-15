using NQuery.Language.Binding;

namespace NQuery.Language.Symbols
{
    public sealed class BadVariableSymbol : VariableSymbol
    {
        public BadVariableSymbol(string name)
            : base(name, KnownTypes.Unknown)
        {
        }
    }
}