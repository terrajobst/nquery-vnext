using NQuery.CodeAnalysis.Symbols;
using NQuery.Metadata;

namespace NQuery.CodeAnalysis.Algebra;

// The logical counterpart of BoundAggregatedValue: the aggregate applied to a
// logical argument expression, producing an output slot.
internal sealed class LogicalAggregatedValue
{
    public LogicalAggregatedValue(ValueSlot output, AggregateSymbol aggregate, AggregateFold fold, LogicalExpression argument)
    {
        ThrowIfNull(output);
        ThrowIfNull(aggregate);
        ThrowIfNull(fold);
        ThrowIfNull(argument);

        Output = output;
        Aggregate = aggregate;
        Fold = fold;
        Argument = argument;
    }

    public ValueSlot Output { get; }

    public AggregateSymbol Aggregate { get; }

    public AggregateFold Fold { get; }

    public LogicalExpression Argument { get; }
}
