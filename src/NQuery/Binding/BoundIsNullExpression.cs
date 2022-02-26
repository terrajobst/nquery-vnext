namespace NQuery.Binding
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

        public BoundIsNullExpression Update(BoundExpression expression)
        {
            if (expression == Expression)
                return this;

            return new BoundIsNullExpression(expression);
        }

        public override string ToString()
        {
            return $"{Expression} IS NULL";
        }
    }
}