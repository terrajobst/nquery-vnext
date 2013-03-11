using System;

using NQuery.Binding;

namespace NQuery.Algebra
{
    internal sealed class AlgebraAllAnySubselect : AlgebraExpression
    {
        private readonly AlgebraExpression _expression;
        private readonly AlgebraRelation _query;
        private readonly BinaryOperatorSignature _signature;

        public AlgebraAllAnySubselect(AlgebraExpression expression, AlgebraRelation query, BinaryOperatorSignature signature)
        {
            _expression = expression;
            _query = query;
            _signature = signature;
        }

        public override AlgebraKind Kind
        {
            get { return AlgebraKind.AllAnySubselect; }
        }

        public override Type Type
        {
            get { return typeof (bool); }
        }

        public AlgebraExpression Expression
        {
            get { return _expression; }
        }

        public AlgebraRelation Query
        {
            get { return _query; }
        }

        public BinaryOperatorSignature Signature
        {
            get { return _signature; }
        }
    }
}