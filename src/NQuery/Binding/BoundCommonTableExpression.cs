using System;

using NQuery.Symbols;

namespace NQuery.Binding
{
    internal sealed class BoundCommonTableExpression : BoundNode
    {
        public BoundCommonTableExpression(CommonTableExpressionSymbol tableSymbol)
        {
            TableSymbol = tableSymbol;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.CommonTableExpression; }
        }

        public CommonTableExpressionSymbol TableSymbol { get; }
    }
}