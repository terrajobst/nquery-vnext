using NQuery.Binding;

namespace NQuery.AlgebraBinding
{
    internal abstract class BoundExpression : BoundNode
    {
        public abstract Type Type { get; }
    }
}