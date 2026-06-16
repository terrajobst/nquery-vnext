namespace NQuery.CodeAnalysis.Binding;

internal sealed class BoundComputedValue
{
    public BoundComputedValue(BoundExpression expression, IBoundValue value)
    {
        Expression = expression;
        Value = value;
    }

    public BoundExpression Expression { get; }

    public IBoundValue Value { get; }
}
