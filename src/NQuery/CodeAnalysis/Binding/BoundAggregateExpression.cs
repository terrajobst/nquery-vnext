using NQuery.CodeAnalysis.Symbols;
using NQuery.Metadata;

namespace NQuery.CodeAnalysis.Binding;

internal sealed class BoundAggregateExpression : BoundExpression
{
    public BoundAggregateExpression(AggregateSymbol aggregate, AggregateFold? fold, BoundExpression argument)
    {
        ThrowIfNull(aggregate);
        ThrowIfNull(argument);

        Symbol = aggregate;
        Fold = fold;
        Argument = argument;
    }

    public override BoundNodeKind Kind
    {
        get { return BoundNodeKind.AggregateExpression; }
    }

    public override Type Type
    {
        get
        {
            return Fold is null
                ? TypeFacts.Unknown
                : Fold.ReturnType;
        }
    }

    public AggregateSymbol Symbol { get; }

    public AggregateSymbol Aggregate
    {
        get { return Symbol; }
    }

    public AggregateFold? Fold { get; }

    public BoundExpression Argument { get; }

    public override string ToString()
    {
        return $"{Symbol.Name}({Argument})";
    }
}
