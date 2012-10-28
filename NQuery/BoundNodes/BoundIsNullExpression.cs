using System;

namespace NQuery.Language.BoundNodes
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
    }
}