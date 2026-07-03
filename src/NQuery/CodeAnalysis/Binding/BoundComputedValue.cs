namespace NQuery.CodeAnalysis.Binding;

internal sealed class BoundComputedValue
{
    public BoundComputedValue(BoundExpression expression, IBoundValue value)
    {
        ThrowIfNull(expression);
        ThrowIfNull(value);

        Expression = expression;
        Value = value;
    }

    public BoundExpression Expression { get; }

    public IBoundValue Value { get; }
}
