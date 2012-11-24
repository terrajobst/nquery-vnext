using System;

using NQuery.Binding;

namespace NQuery.Algebra
{
    internal sealed class AlgebraBinaryExpression : AlgebraExpression
    {
        private readonly AlgebraExpression _left;
        private readonly AlgebraExpression _right;
        private readonly BinaryOperatorSignature _signature;

        public AlgebraBinaryExpression(AlgebraExpression left, AlgebraExpression right, BinaryOperatorSignature signature)
        {
            _left = left;
            _right = right;
            _signature = signature;
        }

        public override AlgebraKind Kind
        {
            get { return AlgebraKind.BinaryExpression; }
        }

        public AlgebraExpression Left
        {
            get { return _left; }
        }

        public AlgebraExpression Right
        {
            get { return _right; }
        }

        public BinaryOperatorSignature Signature
        {
            get { return _signature; }
        }
    }
}