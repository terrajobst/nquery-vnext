using System;

namespace NQuery.Binding
{
    internal sealed class BoundExistsSubselect : BoundExpression
    {
        private readonly BoundQueryRelation _boundQuery;

        public BoundExistsSubselect(BoundQueryRelation boundQuery)
        {
            _boundQuery = boundQuery;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.ExistsSubselect; }
        }

        public BoundQueryRelation BoundQuery
        {
            get { return _boundQuery; }
        }

        public override Type Type
        {
            get { return typeof(bool); }
        }
    }
}