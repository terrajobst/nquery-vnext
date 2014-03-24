using System;

using NQuery.Symbols.Aggregation;

namespace NQuery.Binding
{
    internal sealed class BoundAggregatedValue
    {
        private readonly ValueSlot _output;
        private readonly AggregateSymbol _aggregate;
        private readonly IAggregatable _aggregatable;
        private readonly BoundExpression _argument;

        public BoundAggregatedValue(ValueSlot output, AggregateSymbol aggregate, IAggregatable aggregatable, BoundExpression argument)
        {
            _output = output;
            _aggregate = aggregate;
            _aggregatable = aggregatable;
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

        public IAggregatable Aggregatable
        {
            get { return _aggregatable; }
        }

        public BoundExpression Argument
        {
            get { return _argument; }
        }

        public BoundAggregatedValue Update(ValueSlot output, AggregateSymbol aggregate, IAggregatable aggregatable, BoundExpression argument)
        {
            if (output == _output && aggregate == _aggregate && aggregatable == _aggregatable && argument == _argument)
                return this;

            return new BoundAggregatedValue(output, aggregate, aggregatable, argument);
        }
    }
}