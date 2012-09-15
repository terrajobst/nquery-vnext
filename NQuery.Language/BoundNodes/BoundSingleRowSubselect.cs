using System;
using System.Linq;
using NQuery.Language.Binding;

namespace NQuery.Language.BoundNodes
{
    internal sealed class BoundSingleRowSubselect : BoundExpression
    {
        private readonly BoundQuery _boundQuery;
        private readonly Type _type;

        public BoundSingleRowSubselect(BoundQuery boundQuery)
        {
            _boundQuery = boundQuery;
            var firstColumn = _boundQuery.SelectColumns.FirstOrDefault();
            _type = firstColumn == null
                        ? KnownTypes.Unknown
                        : firstColumn.Expression.Type;
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