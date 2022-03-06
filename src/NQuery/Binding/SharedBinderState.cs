namespace NQuery.Binding
{
    internal sealed class SharedBinderState
    {
        public Dictionary<SyntaxNode, BoundNode> BoundNodeFromSyntaxNode { get; } = new();

        public Dictionary<BoundNode, Binder> BinderFromBoundNode { get; } = new();

        public List<Diagnostic> Diagnostics { get; } = new();

        public ValueSlotFactory ValueSlotFactory { get; } = new();
    }
}