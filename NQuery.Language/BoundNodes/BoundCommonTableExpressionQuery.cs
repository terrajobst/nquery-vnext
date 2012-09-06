using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NQuery.Language.BoundNodes
{
    internal sealed class BoundCommonTableExpressionQuery : BoundQuery
    {
        private readonly ReadOnlyCollection<BoundCommonTableExpression> _commonTableExpressions;
        private readonly BoundQuery _query;

        public BoundCommonTableExpressionQuery(IList<BoundCommonTableExpression> commonTableExpressions, BoundQuery query)
        {
            _commonTableExpressions = new ReadOnlyCollection<BoundCommonTableExpression>(commonTableExpressions);
            _query = query;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.CommonTableExpressionQuery; }
        }

        public override ReadOnlyCollection<BoundSelectColumn> SelectColumns
        {
            get { return _query.SelectColumns; }
        }

        public ReadOnlyCollection<BoundCommonTableExpression> CommonTableExpressions
        {
            get { return _commonTableExpressions; }
        }

        public BoundQuery Query
        {
            get { return _query; }
        }
    }
}