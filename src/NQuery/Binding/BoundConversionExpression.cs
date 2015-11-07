using System;

namespace NQuery.Binding
{
    internal sealed class BoundConversionExpression : BoundExpression
    {
        private readonly Type _type;

        public BoundConversionExpression(BoundExpression expression, Type type, Conversion conversion)
        {
            Expression = expression;
            _type = type;
            Conversion = conversion;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.ConversionExpression; }
        }

        public override Type Type
        {
            get { return _type; }
        }

        public BoundExpression Expression { get; }

        public Conversion Conversion { get; }

        public BoundConversionExpression Update(BoundExpression expression, Type type, Conversion conversion)
        {
            if (expression == Expression && type == _type && conversion == Conversion)
                return this;

            return new BoundConversionExpression(expression, type, conversion);
        }

        public override string ToString()
        {
            return $"CAST({Expression} AS {_type.ToDisplayName()})";
        }
    }
}