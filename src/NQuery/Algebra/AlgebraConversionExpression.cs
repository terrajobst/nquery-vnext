using System;

namespace NQuery.Algebra
{
    internal sealed class AlgebraConversionExpression : AlgebraExpression
    {
        private readonly AlgebraExpression _expression;
        private readonly Conversion _conversion;
        private readonly Type _type;

        public AlgebraConversionExpression(AlgebraExpression expression, Conversion conversion, Type type)
        {
            _expression = expression;
            _conversion = conversion;
            _type = type;
        }

        public override AlgebraKind Kind
        {
            get { return AlgebraKind.ConversionExpression; }
        }

        public override Type Type
        {
            get { return _type; }
        }

        public AlgebraExpression Expression
        {
            get { return _expression; }
        }

        public Conversion Conversion
        {
            get { return _conversion; }
        }
    }
}