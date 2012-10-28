using System;
using System.Collections.Generic;

using NQuery.Symbols;

namespace NQuery.Binding
{
    internal abstract class BindingContext
    {
        public abstract IEnumerable<Symbol> LookupSymbols();
    }
}