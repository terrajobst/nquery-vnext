namespace NQuery.CodeAnalysis.Binding;

// A FROM-clause table reference whose name didn't resolve. It carries no table instance or
// values, so resolution simply yields no symbol downstream. Error compilations never reach
// the algebrizer, so this node is binder-only.
internal sealed class BoundErrorTable : BoundTableReference
{
    public override BoundNodeKind Kind => BoundNodeKind.ErrorTable;

    public override IEnumerable<IBoundValue> GetDefinedValues()
    {
        return [];
    }

    public override IEnumerable<IBoundValue> GetOutputValues()
    {
        return [];
    }

    public override string ToString()
    {
        return @"?";
    }
}
