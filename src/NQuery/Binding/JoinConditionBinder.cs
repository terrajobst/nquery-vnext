using System;
using System.Collections.Generic;

using NQuery.BoundNodes;
using NQuery.Symbols;

namespace NQuery.Binding
{
    internal sealed class JoinConditionBinder : LocalBinder
    {
        public JoinConditionBinder(Binder parent, Dictionary<SyntaxNode, BoundNode> boundNodeFromSynatxNode, Dictionary<BoundNode, Binder> binderFromBoundNode, List<Diagnostic> diagnostics, ValueSlotFactory valueSlotFactory, IEnumerable<Symbol> localSymbols)
            : base(parent, boundNodeFromSynatxNode, binderFromBoundNode, diagnostics, valueSlotFactory, localSymbols)
        {
        }

        protected override bool InOnClause
        {
            get { return true; }
        }
    }
}