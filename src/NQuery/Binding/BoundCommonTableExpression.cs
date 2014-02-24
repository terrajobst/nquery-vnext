using System;

using NQuery.Symbols;

namespace NQuery.Binding
{
    internal sealed class BoundCommonTableExpression : BoundNode
    {
        private readonly CommonTableExpressionSymbol _tableSymbol;

        public BoundCommonTableExpression(CommonTableExpressionSymbol tableSymbol)
        {
            _tableSymbol = tableSymbol;
        }

        public CommonTableExpressionSymbol TableSymbol
        {
            get { return _tableSymbol; }
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.CommonTableExpression; }
        }
    }
}