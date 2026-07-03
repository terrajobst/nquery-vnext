namespace NQuery.CodeAnalysis.Binding;

internal sealed class AggregateArgumentBinder : Binder
{
    public AggregateArgumentBinder(SharedBinderState sharedBinderState, Binder parent)
        : base(sharedBinderState, parent)
    {
        ThrowIfNull(sharedBinderState);
        ThrowIfNull(parent);
    }

    protected override bool InAggregateArgument
    {
        get { return true; }
    }
}
