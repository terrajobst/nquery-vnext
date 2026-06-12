using NQuery.Binding;

namespace NQuery.Refactor.Binding
{
    internal sealed class BoundIsNullExpression : BoundExpression
    {
        public BoundIsNullExpression(BoundExpression expression)
        {
            Expression = expression;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.IsNullExpression; }
        }

        public override Type Type
        {
            get { return typeof(bool); }
        }

        public BoundExpression Expression { get; }

        public override string ToString()
        {
            return $"{Expression} IS NULL";
        }
    }
}