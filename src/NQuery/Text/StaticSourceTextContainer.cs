namespace NQuery.Text
{
    internal class StaticSourceTextContainer : SourceTextContainer
    {
        public StaticSourceTextContainer(SourceText current)
        {
            ArgumentNullException.ThrowIfNull(current);
            ArgumentNullException.ThrowIfNull(current);

            Current = current;
        }

        public override SourceText Current { get; }

        public override event EventHandler<EventArgs> CurrentChanged
        {
            add { }
            remove { }
        }
    }
}