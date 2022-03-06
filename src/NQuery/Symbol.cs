using NQuery.Symbols;

namespace NQuery
{
    public abstract class Symbol
    {
        internal Symbol(string name)
        {
            ArgumentNullException.ThrowIfNull(name);

            Name = name;
        }

        public abstract SymbolKind Kind { get; }

        public string Name { get; }

        public abstract Type Type { get; }

        public sealed override string ToString()
        {
            return SymbolMarkup.ForSymbol(this).ToString();
        }
    }
}