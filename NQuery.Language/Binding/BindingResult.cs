using System.Collections.Generic;
using System.Collections.ObjectModel;
using NQuery.Language;
using NQuery.Language.BoundNodes;

namespace NQuery.Language.Binding
{
    internal sealed class BindingResult
    {
        private readonly SyntaxNode _root;
        private readonly BoundNode _boundRoot;
        private readonly IDictionary<SyntaxNode, BoundNode> _boundNodeFromSynatxNode;
        private readonly IDictionary<BoundNode, BindingContext> _bindingContextFromBoundNode;
        private readonly ReadOnlyCollection<Diagnostic> _diagnostics;

        public BindingResult(SyntaxNode root, BoundNode boundRoot, IDictionary<SyntaxNode, BoundNode> boundNodeFromSynatxNode, IDictionary<BoundNode, BindingContext> bindingContextFromBoundNode, IList<Diagnostic> diagnostics)
        {
            _root = root;
            _boundRoot = boundRoot;
            _boundNodeFromSynatxNode = boundNodeFromSynatxNode;
            _bindingContextFromBoundNode = bindingContextFromBoundNode;
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

        public BindingContext GetBindingContext(SyntaxNode syntaxNode)
        {
            var boundNode = GetBoundNode(syntaxNode);
            return boundNode == null ? null : GetBindingContext(boundNode);
        }

        public BindingContext GetBindingContext(BoundNode boundNode)
        {
            BindingContext result;
            _bindingContextFromBoundNode.TryGetValue(boundNode, out result);
            return result;
        }
    }
}