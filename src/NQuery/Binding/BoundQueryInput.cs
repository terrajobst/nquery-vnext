using System.Collections.Immutable;

namespace NQuery.Binding;

// One input to a set operation (UNION / INTERSECT / EXCEPT), kept relation-free. When the
// inputs need type coercion to a common type, the conversions are captured as ComputedValues
// (expression -> new value) and OutputValues references the resulting values; otherwise
// OutputValues are the input query's own output values and ComputedValues is empty. The
// algebrizer reproduces the legacy Compute/Project wrapping from this.
internal sealed class BoundQueryInput
{
    public BoundQueryInput(BoundQuery query, ImmutableArray<BoundComputedValue> computedValues, ImmutableArray<IBoundValue> outputValues)
    {
        Query = query;
        ComputedValues = computedValues;
        OutputValues = outputValues;
    }

    public BoundQuery Query { get; }

    public ImmutableArray<BoundComputedValue> ComputedValues { get; }

    public ImmutableArray<IBoundValue> OutputValues { get; }
}
