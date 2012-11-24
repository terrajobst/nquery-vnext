using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NQuery.Algebra
{
    internal sealed class AlgebraComputeNode : AlgebraRelation
    {
        private readonly AlgebraRelation _input;
        private readonly ReadOnlyCollection<AlgebraExpression> _expressions;

        public AlgebraComputeNode(AlgebraRelation input, IList<AlgebraExpression> expressions)
        {
            _input = input;
            _expressions = new ReadOnlyCollection<AlgebraExpression>(expressions);
        }

        public override AlgebraKind Kind
        {
            get { return AlgebraKind.Compute; }
        }

        public AlgebraRelation Input
        {
            get { return _input; }
        }

        public ReadOnlyCollection<AlgebraExpression> Expressions
        {
            get { return _expressions; }
        }
    }
}