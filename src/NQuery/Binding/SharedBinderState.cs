using System;
using System.Collections.Generic;

namespace NQuery.Binding
{
    internal sealed class SharedBinderState
    {
        public Dictionary<SyntaxNode, BoundNode> BoundNodeFromSynatxNode { get; } = new Dictionary<SyntaxNode, BoundNode>();

        public Dictionary<BoundNode, Binder> BinderFromBoundNode { get; } = new Dictionary<BoundNode, Binder>();

        public List<Diagnostic> Diagnostics { get; } = new List<Diagnostic>();

        public ValueSlotFactory ValueSlotFactory { get; } = new ValueSlotFactory();
    }
}