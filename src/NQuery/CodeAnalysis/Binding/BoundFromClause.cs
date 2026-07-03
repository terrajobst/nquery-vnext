namespace NQuery.CodeAnalysis.Binding;

internal sealed class BoundFromClause
{
    public BoundFromClause(BoundTableReference root)
    {
        ThrowIfNull(root);

        Root = root;
    }

    public BoundTableReference Root { get; }
}
