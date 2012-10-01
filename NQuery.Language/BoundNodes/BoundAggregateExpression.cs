using System;
using NQuery.Language.Symbols;

namespace NQuery.Language.BoundNodes
{
    internal sealed class BoundAggregateExpression : BoundExpression
    {
        private readonly BoundExpression _argument;
        private readonly AggregateSymbol _aggregate;

        public BoundAggregateExpression(BoundExpression argument, AggregateSymbol aggregate)
        {
            _argument = argument;
            _aggregate = aggregate;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.AggregateExpression; }
        }

        public override Type Type
        {
            get { return _argument.Type; }
        }

        public AggregateSymbol Symbol
        {
            get { return _aggregate; }
        }

        public BoundExpression Argument
        {
            get { return _argument; }
        }

        public AggregateSymbol Aggregate
        {
            get { return _aggregate; }
        }
    }
}