using System;

using NQuery.Symbols;

namespace NQuery.Algebra
{
    internal sealed class AlgebraAggregateExpression : AlgebraExpression
    {
        private readonly AggregateSymbol _symbol;
        private readonly AlgebraExpression _argument;

        public AlgebraAggregateExpression(AggregateSymbol symbol, AlgebraExpression argument)
        {
            _symbol = symbol;
            _argument = argument;
        }

        public override AlgebraKind Kind
        {
            get { return AlgebraKind.AggregateExpression; }
        }

        public AggregateSymbol Symbol
        {
            get { return _symbol; }
        }

        public AlgebraExpression Argument
        {
            get { return _argument; }
        }
    }
}