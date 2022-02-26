namespace NQuery.Iterators
{
    internal abstract class Iterator : IDisposable
    {
        public abstract RowBuffer RowBuffer { get; }

        public abstract void Open();
        public abstract void Dispose();
        public abstract bool Read();
    }
}