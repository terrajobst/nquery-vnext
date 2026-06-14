using NQuery.Symbols.Aggregation;

namespace NQuery.Algebra;

// The logical counterpart of BoundAggregatedValue: the aggregate applied to a
// logical argument expression, producing an output slot.
internal sealed class LogicalAggregatedValue
{
    public LogicalAggregatedValue(ValueSlot output, AggregateSymbol aggregate, IAggregatable aggregatable, LogicalExpression argument)
    {
        Output = output;
        Aggregate = aggregate;
        Aggregatable = aggregatable;
        Argument = argument;
    }

    public ValueSlot Output { get; }

    public AggregateSymbol Aggregate { get; }

    public IAggregatable Aggregatable { get; }

    public LogicalExpression Argument { get; }
}
