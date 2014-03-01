using System;

namespace NQuery.Binding
{
    internal sealed class BoundConversionExpression : BoundExpression
    {
        private readonly BoundExpression _expression;
        private readonly Type _type;
        private readonly Conversion _conversion;

        public BoundConversionExpression(BoundExpression expression, Type type, Conversion conversion)
        {
            _expression = expression;
            _type = type;
            _conversion = conversion;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.ConversionExpression; }
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

        public BoundConversionExpression Update(BoundExpression expression, Type type, Conversion conversion)
        {
            if (expression == _expression && type == _type && conversion == _conversion)
                return this;

            return new BoundConversionExpression(expression, type, conversion);
        }

        public override string ToString()
        {
            return string.Format("CAST({0} AS {1})", _expression, _type.ToDisplayName());
        }
    }
}