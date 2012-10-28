using System;

namespace NQuery.BoundNodes
{
    internal sealed class BoundExistsSubselect : BoundExpression
    {
        private readonly BoundQuery _boundQuery;

        public BoundExistsSubselect(BoundQuery boundQuery)
        {
            _boundQuery = boundQuery;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.ExistsSubselect; }
        }

        public BoundQuery BoundQuery
        {
            get { return _boundQuery; }
        }

        public override Type Type
        {
            get { return typeof(bool); }
        }
    }
}