namespace NQuery.CodeAnalysis.Binding;

internal sealed class BoundSingleRowSubselect : BoundExpression
{
    public BoundSingleRowSubselect(IBoundValue value, BoundQuery query)
    {
        Value = value;
        Query = query;
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

    public override string ToString()
    {
        return $"({Query})";
    }
}
