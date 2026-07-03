namespace NQuery.CodeAnalysis.Binding;

internal sealed class JoinConditionBinder : LocalBinder
{
    public JoinConditionBinder(SharedBinderState sharedBinderState, Binder parent, IEnumerable<Symbol> localSymbols)
        : base(sharedBinderState, parent, localSymbols)
    {
        ThrowIfNull(sharedBinderState);
        ThrowIfNull(parent);
        ThrowIfNull(localSymbols);
    }

    protected override bool InOnClause
    {
        get { return true; }
    }
}
