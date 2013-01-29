using System;
using System.Collections.Generic;

using NQuery.BoundNodes;

namespace NQuery.Binding
{
    internal sealed class WhereClauseBinder : Binder
    {
        public WhereClauseBinder(Binder parent, Dictionary<SyntaxNode, BoundNode> boundNodeFromSynatxNode, Dictionary<BoundNode, Binder> binderFromBoundNode, List<Diagnostic> diagnostics, ValueSlotFactory valueSlotFactory)
            : base(parent, boundNodeFromSynatxNode, binderFromBoundNode, diagnostics, valueSlotFactory)
        {
        }

        protected override bool InWhereClause
        {
            get { return true; }
        }
    }
}