namespace NQuery.Text
{
    public abstract class SourceTextContainer
    {
        public abstract SourceText Current { get; }
        public abstract event EventHandler<EventArgs> CurrentChanged;
    }
}