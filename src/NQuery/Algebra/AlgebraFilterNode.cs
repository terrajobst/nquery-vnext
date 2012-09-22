using System;

using NQuery.BoundNodes;

namespace NQuery.Algebra
{
    internal sealed class AlgebraFilterNode : AlgebraNode
    {
        private readonly AlgebraNode _input;
        private readonly BoundExpression _condition;

        public AlgebraFilterNode(AlgebraNode input, BoundExpression condition)
        {
            _input = input;
            _condition = condition;
        }

        public AlgebraNode Input
        {
            get { return _input; }
        }

        public BoundExpression Condition
        {
            get { return _condition; }
        }

        public override AlgebraKind Kind
        {
            get { return AlgebraKind.Filter; }
        }
    }
}