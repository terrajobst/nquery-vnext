using NQuery.Binding;

namespace NQuery.AlgebraBinding
{
    internal abstract class BoundNode
    {
        public abstract BoundNodeKind Kind { get; }
    }
}