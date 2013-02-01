using System;

namespace NQuery.Binding
{
    internal sealed class BoundIsNullExpression : BoundExpression
    {
        private readonly BoundExpression _expression;

        public BoundIsNullExpression(BoundExpression expression)
        {
            _expression = expression;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.IsNullExpression; }
        }

        public override Type Type
        {
            get { return typeof(bool); }
        }

        public BoundExpression Expression
        {
            get { return _expression; }
        }

        public BoundExpression Update(BoundExpression expression)
        {
            return _expression == expression
                       ? this
                       : new BoundIsNullExpression(expression);
        }

        public override string ToString()
        {
            return string.Format("{0} IS NULL", _expression);
        }
    }
}