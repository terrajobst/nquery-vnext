using System;

namespace NQuery.Language.BoundNodes
{
    internal sealed class BoundCastExpression : BoundExpression
    {
        private readonly BoundExpression _expression;
        private readonly Type _type;
        private readonly Conversion _conversion;

        public BoundCastExpression(BoundExpression expression, Type type, Conversion conversion)
        {
            _expression = expression;
            _type = type;
            _conversion = conversion;
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

        public Conversion Conversion
        {
            get { return _conversion; }
        }
    }
}