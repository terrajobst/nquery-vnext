using System;

using NQuery.BoundNodes;

namespace NQuery.Algebra
{
    internal sealed class AlgebraComputeNode : AlgebraNode
    {
        private readonly AlgebraNode _input;
        private readonly BoundExpression[] _resultExpressions;

        public AlgebraComputeNode(AlgebraNode input, BoundExpression[] resultExpressions)
        {
            _input = input;
            _resultExpressions = resultExpressions;
        }

        public AlgebraNode Input
        {
            get { return _input; }
        }

        public BoundExpression[] ResultExpressions
        {
            get { return _resultExpressions; }
        }

        public override AlgebraKind Kind
        {
            get { return AlgebraKind.Compute; }
        }
    }
}