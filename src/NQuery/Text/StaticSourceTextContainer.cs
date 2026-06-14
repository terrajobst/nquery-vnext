namespace NQuery.Text;

internal class StaticSourceTextContainer : SourceTextContainer
{
    public StaticSourceTextContainer(SourceText current)
    {
        ThrowIfNull(current);
        ThrowIfNull(current);

        Current = current;
    }

    public override SourceText Current { get; }

    public override event EventHandler<EventArgs> CurrentChanged
    {
        add { }
        remove { }
    }
}