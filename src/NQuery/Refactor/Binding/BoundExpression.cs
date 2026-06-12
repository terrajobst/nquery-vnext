using NQuery.Binding;

namespace NQuery.Refactor.Binding
{
    internal abstract class BoundExpression : BoundNode
    {
        public abstract Type Type { get; }
    }
}