using System;

namespace NQuery.Algebra
{
    internal sealed class AlgebraCombinedQuery : AlgebraRelation
    {
        private readonly AlgebraRelation _left;
        private readonly AlgebraRelation _right;
        private readonly AlgebraQueryCombinator _combinator;

        public AlgebraCombinedQuery(AlgebraRelation left, AlgebraRelation right, AlgebraQueryCombinator combinator)
        {
            _left = left;
            _right = right;
            _combinator = combinator;
        }

        public override AlgebraKind Kind
        {
            get { return AlgebraKind.BinaryQuery; }
        }

        public AlgebraRelation Left
        {
            get { return _left; }
        }

        public AlgebraRelation Right
        {
            get { return _right; }
        }

        public AlgebraQueryCombinator Combinator
        {
            get { return _combinator; }
        }
    }
}