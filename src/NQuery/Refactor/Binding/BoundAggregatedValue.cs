using NQuery.Symbols.Aggregation;

using NQuery.Binding;

namespace NQuery.Refactor.Binding
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

    }
}