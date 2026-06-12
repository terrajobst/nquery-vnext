using NQuery.Symbols.Aggregation;

namespace NQuery.Refactor.Binding
{
    internal sealed class BoundAggregatedValue
    {
        public BoundAggregatedValue(IBoundValue output, AggregateSymbol aggregate, IAggregatable aggregatable, BoundExpression argument)
        {
            Output = output;
            Aggregate = aggregate;
            Aggregatable = aggregatable;
            Argument = argument;
        }

        public IBoundValue Output { get; }

        public AggregateSymbol Aggregate { get; }

        public IAggregatable Aggregatable { get; }

        public BoundExpression Argument { get; }
    }
}
