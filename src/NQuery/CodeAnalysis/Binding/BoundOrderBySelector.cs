namespace NQuery.CodeAnalysis.Binding;

internal readonly struct BoundOrderBySelector
{
    public BoundOrderBySelector(IBoundValue value, BoundComputedValueWithSyntax? computedValue)
    {
        Value = value;
        ComputedValue = computedValue;
    }

    public IBoundValue Value { get; }

    public BoundComputedValueWithSyntax? ComputedValue { get; }
}
