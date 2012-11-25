using System;
using System.Collections.Generic;

using NQuery.BoundNodes;
using NQuery.Symbols;

namespace NQuery.Binding
{
    internal sealed class LocalBinder : Binder
    {
        private readonly IEnumerable<Symbol> _localSymbols;

        public LocalBinder(Binder parent, Dictionary<SyntaxNode, BoundNode> boundNodeFromSynatxNode, Dictionary<BoundNode, Binder> binderFromBoundNode, List<Diagnostic> diagnostics, IEnumerable<Symbol> localSymbols)
            : base(parent, boundNodeFromSynatxNode, binderFromBoundNode, diagnostics)
        {
            _localSymbols = localSymbols;
        }

        public override IEnumerable<Symbol> GetLocalSymbols()
        {
            foreach (var symbol in _localSymbols)
            {
                yield return symbol;

                var table = symbol as TableInstanceSymbol;
                if (table != null)
                {
                    foreach (var columnInstance in table.ColumnInstances)
                        yield return columnInstance;
                }
            }
        }

        public override IEnumerable<PropertySymbol> LookupProperties(Type type)
        {
            return Parent.LookupProperties(type);
        }

        public override IEnumerable<MethodSymbol> LookupMethods(Type type)
        {
            return Parent.LookupMethods(type);
        }
    }
}