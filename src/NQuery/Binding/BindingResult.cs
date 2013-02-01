using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NQuery.Binding
{
    internal sealed class BindingResult
    {
        private readonly SyntaxNode _root;
        private readonly BoundNode _boundRoot;
        private readonly IDictionary<SyntaxNode, BoundNode> _boundNodeFromSynatxNode;
        private readonly IDictionary<BoundNode, Binder> _binderFromBoundNode;
        private readonly ReadOnlyCollection<Diagnostic> _diagnostics;

        public BindingResult(SyntaxNode root, BoundNode boundRoot, IDictionary<SyntaxNode, BoundNode> boundNodeFromSynatxNode, IDictionary<BoundNode, Binder> binderFromBoundNode, IList<Diagnostic> diagnostics)
        {
            _root = root;
            _boundRoot = boundRoot;
            _boundNodeFromSynatxNode = boundNodeFromSynatxNode;
            _binderFromBoundNode = binderFromBoundNode;
            _diagnostics = new ReadOnlyCollection<Diagnostic>(diagnostics);
        }

        public SyntaxNode Root
        {
            get { return _root; }
        }

        public BoundNode BoundRoot
        {
            get { return _boundRoot; }
        }

        public Binder RootBinder
        {
            get { return _binderFromBoundNode[_boundRoot]; }
        }

        public ReadOnlyCollection<Diagnostic> Diagnostics
        {
            get { return _diagnostics; }
        }

        public BoundNode GetBoundNode(SyntaxNode syntaxNode)
        {
            BoundNode result;
            _boundNodeFromSynatxNode.TryGetValue(syntaxNode, out result);
            return result;
        }

        public Binder GetBinder(SyntaxNode syntaxNode)
        {
            var boundNode = GetBoundNode(syntaxNode);
            return boundNode == null ? null : GetBinder(boundNode);
        }

        public Binder GetBinder(BoundNode boundNode)
        {
            Binder result;
            _binderFromBoundNode.TryGetValue(boundNode, out result);
            return result;
        }
    }
}