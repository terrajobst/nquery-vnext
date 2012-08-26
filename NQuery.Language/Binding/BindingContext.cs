using System;
using System.Collections.Generic;
using System.Linq;
using NQuery.Language.Symbols;

namespace NQuery.Language.Binding
{
    internal abstract class BindingContext
    {
        public abstract IEnumerable<Symbol> LookupSymbols();

        public IEnumerable<Symbol> LookupSymbols(string name, bool matchCase)
        {
            var ordinalIgnoreCase = matchCase
                                            ? StringComparison.Ordinal
                                            : StringComparison.OrdinalIgnoreCase;

            return LookupSymbols().Where(t => t.Name.Equals(name, ordinalIgnoreCase));
        }
    }
}