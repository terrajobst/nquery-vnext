using System;
using System.Linq;

namespace NQuery.Binding
{
    internal sealed class BoundSingleRowSubselect : BoundExpression
    {
        private readonly BoundQueryRelation _query;
        private readonly Type _type;

        public BoundSingleRowSubselect(BoundQueryRelation query)
        {
            _query = query;
            var firstColumn = _query.OutputColumns.FirstOrDefault();
            _type = firstColumn == null
                        ? TypeFacts.Unknown
                        : firstColumn.Type;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.SingleRowSubselect; }
        }

        public BoundQueryRelation Query
        {
            get { return _query; }
        }

        public override Type Type
        {
            get { return _type; }
        }
    }
}