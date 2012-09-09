using System.Collections.Generic;
using NQuery.Language.Symbols;

namespace NQuery.Language.Binding
{
    internal abstract class BindingContext
    {
        public abstract IEnumerable<Symbol> LookupSymbols();
    }
}