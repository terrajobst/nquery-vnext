using System.Collections.Immutable;

namespace NQuery.Binding
{
    internal sealed class BindingResult
    {
        private readonly IDictionary<SyntaxNode, BoundNode> _boundNodeFromSyntaxNode;
        private readonly IDictionary<BoundNode, Binder> _binderFromBoundNode;

        public BindingResult(SyntaxNode root, BoundNode boundRoot, IDictionary<SyntaxNode, BoundNode> boundNodeFromSyntaxNode, IDictionary<BoundNode, Binder> binderFromBoundNode, IList<Diagnostic> diagnostics)
        {
            Root = root;
            BoundRoot = boundRoot;
            _boundNodeFromSyntaxNode = boundNodeFromSyntaxNode;
            _binderFromBoundNode = binderFromBoundNode;
            Diagnostics = diagnostics.ToImmutableArray();
        }

        public SyntaxNode Root { get; }

        public BoundNode BoundRoot { get; }

        public Binder RootBinder
        {
            get { return _binderFromBoundNode[BoundRoot]; }
        }

        public ImmutableArray<Diagnostic> Diagnostics { get; }

        public BoundNode GetBoundNode(SyntaxNode syntaxNode)
        {
            _boundNodeFromSyntaxNode.TryGetValue(syntaxNode, out var result);
            return result;
        }

        public Binder GetBinder(SyntaxNode syntaxNode)
        {
            var boundNode = GetBoundNode(syntaxNode);
            return boundNode is null ? null : GetBinder(boundNode);
        }

        public Binder GetBinder(BoundNode boundNode)
        {
            _binderFromBoundNode.TryGetValue(boundNode, out var result);
            return result;
        }
    }
}