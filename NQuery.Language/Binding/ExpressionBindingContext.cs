using System.Collections.Generic;
using NQuery.Language.Symbols;

namespace NQuery.Language.Binding
{
    internal sealed class ExpressionBindingContext : BindingContext
    {
        private readonly BindingContext _parent;

        public ExpressionBindingContext(BindingContext parent)
        {
            _parent = parent;
        }

        public override IEnumerable<Symbol> LookupSymbols()
        {
            foreach (var lookupSymbol in _parent.LookupSymbols())
            {
                yield return lookupSymbol;

                var tableInstance = lookupSymbol as TableInstanceSymbol;
                if (tableInstance != null)
                {
                    foreach (var columnInstance in tableInstance.ColumnInstances)
                        yield return columnInstance;
                }
            }
        }
    }
}