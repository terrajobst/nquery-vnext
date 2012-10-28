using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.Symbols;

namespace NQuery.Binding
{
    internal sealed class AdditionalSymbolsBindingContext : BindingContext
    {
        private readonly BindingContext _parent;
        private readonly IEnumerable<Symbol> _symbols;

        public AdditionalSymbolsBindingContext(BindingContext parent, IEnumerable<Symbol> symbols)
        {
            _parent = parent;
            _symbols = symbols;
        }

        public override IEnumerable<Symbol> LookupSymbols()
        {
            return _parent.LookupSymbols().Concat(_symbols);
        }
    }
}