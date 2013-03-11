using System;

namespace NQuery.Algebra
{
    internal sealed class AlgebraSingleRowSubselect : AlgebraExpression
    {
        private readonly AlgebraRelation _query;

        public AlgebraSingleRowSubselect(AlgebraRelation query)
        {
            _query = query;
        }

        public override Type Type
        {
            get { return TypeFacts.Unknown; }
        }

        public override AlgebraKind Kind
        {
            get { return AlgebraKind.SingleRowSubselect; }
        }

        public AlgebraRelation Query
        {
            get { return _query; }
        }
    }
}