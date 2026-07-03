namespace NQuery.CodeAnalysis.Binding;

// References a value by its binder identity. The algebrizer maps the IBoundValue to the
// ValueSlot that carries it. (Was BoundValueSlotExpression -- the binder no longer deals in
// slots.)
internal sealed class BoundValueExpression : BoundExpression
{
    public BoundValueExpression(IBoundValue value)
    {
        ThrowIfNull(value);

        Value = value;
    }

    public override BoundNodeKind Kind
    {
        get { return BoundNodeKind.ValueExpression; }
    }

    public override Type Type
    {
        get { return Value.Type; }
    }

    public IBoundValue Value { get; }

    public override string ToString()
    {
        return Value.ToString()!;
    }
}
