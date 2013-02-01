using System;

using NQuery.Symbols;

namespace NQuery.Binding
{
    internal sealed class BoundAggregateExpression : BoundExpression
    {
        private readonly AggregateSymbol _aggregate;
        private readonly BoundExpression _argument;

        public BoundAggregateExpression(AggregateSymbol aggregate, BoundExpression argument)
        {
            _aggregate = aggregate;
            _argument = argument;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.AggregateExpression; }
        }

        public override Type Type
        {
            // TODO: That's not correct. The aggregate symbol needs to create an aggregator that has a type.
            get { return _argument.Type; }
        }

        public AggregateSymbol Symbol
        {
            get { return _aggregate; }
        }

        public AggregateSymbol Aggregate
        {
            get { return _aggregate; }
        }

        public BoundExpression Argument
        {
            get { return _argument; }
        }

        public override string ToString()
        {
            return string.Format("{0}({1})", _aggregate.Name, _argument);
        }
    }
}