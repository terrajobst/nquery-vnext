using System;

namespace NQuery.Algebra
{
    internal sealed class AlgebraExistsSubselect : AlgebraExpression
    {
        private readonly AlgebraRelation _query;

        public AlgebraExistsSubselect(AlgebraRelation query)
        {
            _query = query;
        }

        public override AlgebraKind Kind
        {
            get { return AlgebraKind.ExistsSubselect; }
        }

        public AlgebraRelation Query
        {
            get { return _query; }
        }
    }
}