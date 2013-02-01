using System;
using System.Collections.Generic;

namespace NQuery.Binding
{
    internal sealed class SharedBinderState
    {
        private readonly Dictionary<SyntaxNode, BoundNode> _boundNodeFromSynatxNode = new Dictionary<SyntaxNode, BoundNode>();
        private readonly Dictionary<BoundNode, Binder> _binderFromBoundNode = new Dictionary<BoundNode, Binder>();
        private readonly List<Diagnostic> _diagnostics = new List<Diagnostic>();
        private readonly ValueSlotFactory _valueSlotFactory = new ValueSlotFactory();

        public Dictionary<SyntaxNode, BoundNode> BoundNodeFromSynatxNode
        {
            get { return _boundNodeFromSynatxNode; }
        }

        public Dictionary<BoundNode, Binder> BinderFromBoundNode
        {
            get { return _binderFromBoundNode; }
        }

        public List<Diagnostic> Diagnostics
        {
            get { return _diagnostics; }
        }

        public ValueSlotFactory ValueSlotFactory
        {
            get { return _valueSlotFactory; }
        }
    }
}