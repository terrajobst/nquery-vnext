using System.Collections;
using System.Collections.Immutable;

namespace NQuery.Binding
{
    internal sealed class SymbolTable : IEnumerable<Symbol>
    {
        public static readonly SymbolTable Empty = new(Enumerable.Empty<Symbol>());

        private readonly ImmutableArray<Symbol> _symbols;
        private readonly ILookup<string, Symbol> _lookup;

        private SymbolTable(IEnumerable<Symbol> symbols)
        {
            _symbols = symbols.ToImmutableArray();
            _lookup = _symbols.ToLookup(s => s.Name, StringComparer.OrdinalIgnoreCase);
        }

        public static SymbolTable Create(IEnumerable<Symbol> symbols)
        {
            return new SymbolTable(symbols);
        }

        public IEnumerable<Symbol> Lookup(string name, bool caseSensitive)
        {
            return caseSensitive
                       ? _lookup[name].Where(s => string.Equals(s.Name, name, StringComparison.Ordinal))
                       : _lookup[name];
        }

        public IEnumerator<Symbol> GetEnumerator()
        {
            return _symbols.AsEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}