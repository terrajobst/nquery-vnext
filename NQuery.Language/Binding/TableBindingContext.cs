using System.Collections.Generic;
using System.Linq;
using NQuery.Language.BoundNodes;
using NQuery.Language.Symbols;

namespace NQuery.Language.Binding
{
    internal sealed class TableBindingContext : BindingContext
    {
        private readonly BindingContext _parent;
        private readonly BoundTableReference _tableReference;

        public TableBindingContext(BindingContext parent, BoundTableReference tableReference)
        {
            _parent = parent;
            _tableReference = tableReference;
        }

        public override IEnumerable<Symbol> LookupSymbols()
        {
            var parentSymbols = _parent.LookupSymbols();
            var tableSymbols = _tableReference.GetDeclaredTableInstances();
            return parentSymbols.Concat(tableSymbols);
        }
    }
}