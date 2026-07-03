using System.Collections.Immutable;

namespace NQuery.CodeAnalysis.Binding;

internal sealed class BoundExistsSubselect : BoundExpression
{
    public BoundExistsSubselect(BoundQuery query)
        : this(query, [], null)
    {
    }

    // ALL/ANY subselects are rewritten into an EXISTS over the subquery with an extra
    // predicate (and possibly a computed value for a type conversion). Rather than lower that
    // into Compute/Filter relations at bind time, we carry the resolved pieces and let the
    // algebrizer apply them over the lowered query.
    public BoundExistsSubselect(BoundQuery query, ImmutableArray<BoundComputedValue> computedValues, BoundExpression? filter)
    {
        ThrowIfNull(query);

        Query = query;
        ComputedValues = computedValues;
        Filter = filter;
    }

    public override BoundNodeKind Kind
    {
        get { return BoundNodeKind.ExistsSubselect; }
    }

    public BoundQuery Query { get; }

    public ImmutableArray<BoundComputedValue> ComputedValues { get; }

    public BoundExpression? Filter { get; }

    public override Type Type
    {
        get { return typeof(bool); }
    }

    public override string ToString()
    {
        return $"EXISTS ({Query})";
    }
}
