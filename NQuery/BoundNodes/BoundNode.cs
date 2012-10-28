namespace NQuery.Language.BoundNodes
{
    internal abstract class BoundNode
    {
        public abstract BoundNodeKind Kind { get; }
    }
}