using NQuery.CodeAnalysis.Symbols;
using NQuery.Metadata;

namespace NQuery.CodeAnalysis.Binding;

internal sealed class BoundAggregatedValue
{
    public BoundAggregatedValue(IBoundValue output, AggregateSymbol aggregate, AggregateFold? fold, BoundExpression argument)
    {
        Output = output;
        Aggregate = aggregate;
        Fold = fold;
        Argument = argument;
    }

    public IBoundValue Output { get; }

    public AggregateSymbol Aggregate { get; }

    public AggregateFold? Fold { get; }

    public BoundExpression Argument { get; }
}
