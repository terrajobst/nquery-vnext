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

        public CommonTableExpressionSymbol TableSymbol { get; }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.CommonTableExpression; }
        }
    }
}