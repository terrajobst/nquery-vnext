namespace NQuery.Binding;

// A FROM-clause table reference whose name didn't resolve. It carries no table instance or
// values, so resolution simply yields no symbol downstream. Error compilations never reach
// the algebrizer, so this node is binder-only.
internal sealed class BoundErrorTable : BoundTableReference
{
    public override BoundNodeKind Kind => BoundNodeKind.ErrorTable;

    public override IEnumerable<IBoundValue> GetDefinedValues()
    {
        return Enumerable.Empty<IBoundValue>();
    }

    public override IEnumerable<IBoundValue> GetOutputValues()
    {
        return Enumerable.Empty<IBoundValue>();
    }

    public override string ToString()
    {
        return @"?";
    }
}
