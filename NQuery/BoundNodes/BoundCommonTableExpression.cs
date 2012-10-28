using System;

using NQuery.Symbols;

namespace NQuery.BoundNodes
{
    internal sealed class BoundCommonTableExpression : BoundNode
    {
        private readonly CommonTableExpressionSymbol _tableSymbol;
        private readonly BoundQuery _query;

        public BoundCommonTableExpression(CommonTableExpressionSymbol tableSymbol, BoundQuery query)
        {
            _tableSymbol = tableSymbol;
            _query = query;
        }

        public CommonTableExpressionSymbol TableSymbol
        {
            get { return _tableSymbol; }
        }

        public BoundQuery Query
        {
            get { return _query; }
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.CommonTableExpression; }
        }
    }
}