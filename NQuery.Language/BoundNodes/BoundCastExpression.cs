using System;

namespace NQuery.Language.BoundNodes
{
    internal sealed class BoundCastExpression : BoundExpression
    {
        private readonly BoundExpression _expression;
        private readonly Type _type;

        public BoundCastExpression(BoundExpression expression, Type type)
        {
            _expression = expression;
            _type = type;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.CastExpression; }
        }

        public override Type Type
        {
            get { return _type; }
        }

        public BoundExpression Expression
        {
            get { return _expression; }
        }
    }
}