using System;

namespace NQuery.Binding
{
    internal sealed class BoundExistsSubselect : BoundExpression
    {
        private readonly BoundQueryRelation _query;

        public BoundExistsSubselect(BoundQueryRelation query)
        {
            _query = query;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.ExistsSubselect; }
        }

        public BoundQueryRelation Query
        {
            get { return _query; }
        }

        public override Type Type
        {
            get { return typeof(bool); }
        }
    }
}