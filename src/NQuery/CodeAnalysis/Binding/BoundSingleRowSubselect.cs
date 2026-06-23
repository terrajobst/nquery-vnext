namespace NQuery.CodeAnalysis.Binding;

internal sealed class BoundSingleRowSubselect : BoundExpression
{
    public BoundSingleRowSubselect(IBoundValue value, BoundQuery query, BoundAggregatedValue valueAggregate, BoundAggregatedValue countAggregate)
    {
        Value = value;
        Query = query;
        ValueAggregate = valueAggregate;
        CountAggregate = countAggregate;
    }

    public override BoundNodeKind Kind
    {
        get { return BoundNodeKind.SingleRowSubselect; }
    }

    public override Type Type
    {
        get { return Value.Type; }
    }

    public IBoundValue Value { get; }

    public BoundQuery Query { get; }

    // A scalar subquery must yield at most one row. When the algebrizer can't prove that from the
    // relation's shape, it guards the subquery with these two aggregates (with no GROUP BY, so each
    // yields a single row carrying both the value and the count): ValueAggregate (ANY) collapses the
    // zero-or-one surviving rows to the scalar result, and CountAggregate detects the error case so
    // the algebrizer can assert count <= 1. The binder creates them so symbol creation and aggregate
    // binding stay out of the algebrizer; they go unused when the algebrizer proves at-most-one-row.
    public BoundAggregatedValue ValueAggregate { get; }

    public BoundAggregatedValue CountAggregate { get; }

    public override string ToString()
    {
        return $"({Query})";
    }
}
