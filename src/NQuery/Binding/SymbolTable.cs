using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using NQuery.Symbols;

namespace NQuery.Binding
{
    internal sealed class SymbolTable : IEnumerable<Symbol>
    {
        public static readonly SymbolTable Empty = new SymbolTable(new Symbol[0]);

        private readonly Symbol[] _symbols;
        private readonly ILookup<string, Symbol> _lookup;

        private SymbolTable(Symbol[] symbols)
        {
            _symbols = symbols;
            _lookup = symbols.ToLookup(s => s.Name, StringComparer.OrdinalIgnoreCase);
        }

        public static SymbolTable Create(IEnumerable<Symbol> symbols)
        {
            return new SymbolTable(symbols.ToArray());
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