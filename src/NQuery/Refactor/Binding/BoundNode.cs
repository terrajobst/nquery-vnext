using NQuery.Binding;

namespace NQuery.Refactor.Binding
{
    internal abstract class BoundNode
    {
        public abstract BoundNodeKind Kind { get; }
    }
}