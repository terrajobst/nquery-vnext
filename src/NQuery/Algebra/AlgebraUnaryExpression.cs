using System;

using NQuery.Binding;

namespace NQuery.Algebra
{
    internal sealed class AlgebraUnaryExpression : AlgebraExpression
    {
        private readonly AlgebraExpression _expression;
        private readonly UnaryOperatorSignature _signature;

        public AlgebraUnaryExpression(AlgebraExpression expression, UnaryOperatorSignature signature)
        {
            _expression = expression;
            _signature = signature;
        }

        public override AlgebraKind Kind
        {
            get { return AlgebraKind.UnaryExpression; }
        }

        public AlgebraExpression Expression
        {
            get { return _expression; }
        }

        public UnaryOperatorSignature Signature
        {
            get { return _signature; }
        }
    }
}