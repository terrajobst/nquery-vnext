using System;
using System.Collections.Generic;

using NQuery.BoundNodes;

namespace NQuery.Binding
{
    internal sealed class AggregateArgumentBinder : Binder
    {
        public AggregateArgumentBinder(Binder parent, Dictionary<SyntaxNode, BoundNode> boundNodeFromSynatxNode, Dictionary<BoundNode, Binder> binderFromBoundNode, List<Diagnostic> diagnostics, ValueSlotFactory valueSlotFactory)
            : base(parent, boundNodeFromSynatxNode, binderFromBoundNode, diagnostics, valueSlotFactory)
        {
        }

        protected override bool InAggregateArgument
        {
            get { return true; }
        }
    }
}