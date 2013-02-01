using System;
using System.Linq;

namespace NQuery.Binding
{
    internal sealed class BoundSingleRowSubselect : BoundExpression
    {
        private readonly BoundQuery _boundQuery;
        private readonly Type _type;

        public BoundSingleRowSubselect(BoundQuery boundQuery)
        {
            _boundQuery = boundQuery;
            var firstColumn = _boundQuery.OutputColumns.FirstOrDefault();
            _type = firstColumn == null
                        ? TypeFacts.Unknown
                        : firstColumn.Type;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.SingleRowSubselect; }
        }

        public BoundQuery BoundQuery
        {
            get { return _boundQuery; }
        }

        public override Type Type
        {
            get { return _type; }
        }
    }
}