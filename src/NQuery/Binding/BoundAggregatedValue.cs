using System;

using NQuery.Symbols.Aggregation;

namespace NQuery.Binding
{
    internal sealed class BoundAggregatedValue
    {
        public BoundAggregatedValue(ValueSlot output, AggregateSymbol aggregate, IAggregatable aggregatable, BoundExpression argument)
        {
            Output = output;
            Aggregate = aggregate;
            Aggregatable = aggregatable;
            Argument = argument;
        }

        public ValueSlot Output { get; }

        public AggregateSymbol Aggregate { get; }

        public IAggregatable Aggregatable { get; }

        public BoundExpression Argument { get; }

        public BoundAggregatedValue Update(ValueSlot output, AggregateSymbol aggregate, IAggregatable aggregatable, BoundExpression argument)
        {
            if (output == Output && aggregate == Aggregate && aggregatable == Aggregatable && argument == Argument)
                return this;

            return new BoundAggregatedValue(output, aggregate, aggregatable, argument);
        }
    }
}