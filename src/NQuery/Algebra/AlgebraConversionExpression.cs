using System;

namespace NQuery.Algebra
{
    internal sealed class AlgebraConversionExpression : AlgebraExpression
    {
        private readonly AlgebraExpression _expression;
        private readonly Conversion _conversion;

        public AlgebraConversionExpression(AlgebraExpression expression, Conversion conversion)
        {
            _expression = expression;
            _conversion = conversion;
        }

        public override AlgebraKind Kind
        {
            get { return AlgebraKind.ConversionExpression; }
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