using System;

namespace NQuery.Algebra
{
    internal sealed class AlgebraFilterNode : AlgebraRelation
    {
        private readonly AlgebraRelation _input;
        private readonly AlgebraExpression _condition;

        public AlgebraFilterNode(AlgebraRelation input, AlgebraExpression condition)
        {
            _input = input;
            _condition = condition;
        }

        public override AlgebraKind Kind
        {
            get { return AlgebraKind.Filter; }
        }

        public AlgebraRelation Input
        {
            get { return _input; }
        }

        public AlgebraExpression Condition
        {
            get { return _condition; }
        }
    }
}