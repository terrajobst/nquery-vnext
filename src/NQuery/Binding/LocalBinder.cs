﻿using NQuery.Symbols;

namespace NQuery.Binding
{
    internal class LocalBinder : Binder
    {
        public LocalBinder(SharedBinderState sharedBinderState, Binder parent, IEnumerable<Symbol> localSymbols)
            : base(sharedBinderState, parent)
        {
            LocalSymbols = SymbolTable.Create(ExpandTableInstances(localSymbols));
        }

        public override SymbolTable LocalSymbols { get; }

        private static IEnumerable<Symbol> ExpandTableInstances(IEnumerable<Symbol> symbols)
        {
            foreach (var symbol in symbols)
            {
                yield return symbol;

                if (symbol is TableInstanceSymbol table)
                {
                    foreach (var columnInstance in table.ColumnInstances)
                        yield return columnInstance;
                }
            }
        }
    }
}