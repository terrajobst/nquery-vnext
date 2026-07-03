namespace NQuery.CodeAnalysis.Binding;

internal sealed class WhereClauseBinder : Binder
{
    public WhereClauseBinder(SharedBinderState sharedBinderState, Binder parent)
        : base(sharedBinderState, parent)
    {
        ThrowIfNull(sharedBinderState);
        ThrowIfNull(parent);
    }

    protected override bool InWhereClause
    {
        get { return true; }
    }
}
