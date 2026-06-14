using NQuery.Binding;

namespace NQuery.Binding
{
    internal sealed class SharedBinderState
    {
        public Dictionary<SyntaxNode, BoundNode> BoundNodeFromSyntaxNode { get; } = new();

        public Dictionary<BoundNode, Binder> BinderFromBoundNode { get; } = new();

        public List<Diagnostic> Diagnostics { get; } = new();

        public BoundValueFactory ValueFactory { get; } = new();
    }
}