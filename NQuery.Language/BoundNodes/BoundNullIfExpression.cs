using System;

namespace NQuery.Language.BoundNodes
{
    internal sealed class BoundNullIfExpression : BoundExpression
    {
        private readonly BoundExpression _leftExpression;
        private readonly BoundExpression _rightExpression;

        public BoundNullIfExpression(BoundExpression leftExpression, BoundExpression rightExpression)
        {
            _leftExpression = leftExpression;
            _rightExpression = rightExpression;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.NullIfExpression; }
        }

        public override Type Type
        {
            get { return _leftExpression.Type; }
        }

        public BoundExpression LeftExpression
        {
            get { return _leftExpression; }
        }

        public BoundExpression RightExpression
        {
            get { return _rightExpression; }
        }
    }
}