using System;

using NQuery.Symbols;

namespace NQuery.Binding
{
    internal struct BoundAggregatedValue
    {
        private readonly ValueSlot _output;
        private readonly AggregateSymbol _aggregate;
        private readonly BoundExpression _argument;

        public BoundAggregatedValue(ValueSlot output, AggregateSymbol aggregate, BoundExpression argument)
        {
            _output = output;
            _aggregate = aggregate;
            _argument = argument;
        }

        public ValueSlot Output
        {
            get { return _output; }
        }

        public AggregateSymbol Aggregate
        {
            get { return _aggregate; }
        }

        public BoundExpression Argument
        {
            get { return _argument; }
        }
    }
}