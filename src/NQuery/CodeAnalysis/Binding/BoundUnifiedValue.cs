using System.Collections.Immutable;

namespace NQuery.CodeAnalysis.Binding;

internal sealed class BoundUnifiedValue
{
    public BoundUnifiedValue(IBoundValue value, IEnumerable<IBoundValue> inputValues)
    {
        Value = value;
        InputValues = inputValues.ToImmutableArray();
    }

    public IBoundValue Value { get; }

    public ImmutableArray<IBoundValue> InputValues { get; }
}
