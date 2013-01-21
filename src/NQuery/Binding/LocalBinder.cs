using System;
using System.Collections.Generic;

using NQuery.BoundNodes;
using NQuery.Symbols;

namespace NQuery.Binding
{
    internal class LocalBinder : Binder
    {
        private readonly SymbolTable _localSymbols;

        public LocalBinder(Binder parent, Dictionary<SyntaxNode, BoundNode> boundNodeFromSynatxNode, Dictionary<BoundNode, Binder> binderFromBoundNode, List<Diagnostic> diagnostics, ValueSlotFactory valueSlotFactory, IEnumerable<Symbol> localSymbols)
            : base(parent, boundNodeFromSynatxNode, binderFromBoundNode, diagnostics, valueSlotFactory)
        {
            _localSymbols = SymbolTable.Create(ExpandTableInstances(localSymbols));
        }

        public override SymbolTable LocalSymbols
        {
            get { return _localSymbols; }
        }

        private static IEnumerable<Symbol> ExpandTableInstances(IEnumerable<Symbol> symbols)
        {
            foreach (var symbol in symbols)
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
    }
}