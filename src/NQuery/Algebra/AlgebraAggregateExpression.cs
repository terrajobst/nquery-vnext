using System;

using NQuery.Symbols;

namespace NQuery.Algebra
{
    internal sealed class AlgebraAggregateExpression : AlgebraExpression
    {
        private readonly AlgebraExpression _argument;
        private readonly AggregateSymbol _symbol;

        public AlgebraAggregateExpression(AlgebraExpression argument, AggregateSymbol symbol)
        {
            _argument = argument;
            _symbol = symbol;
        }

        public override AlgebraKind Kind
        {
            get { return AlgebraKind.AggregateExpression; }
        }

        public AlgebraExpression Argument
        {
            get { return _argument; }
        }

        public AggregateSymbol Symbol
        {
            get { return _symbol; }
        }
    }
}