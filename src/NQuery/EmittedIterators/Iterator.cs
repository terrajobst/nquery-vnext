#nullable enable

namespace NQuery.EmittedIterators
{
    // The runtime cursor of the new pipeline. It is intentionally a separate type
    // from NQuery.Iterators.Iterator so the emitted layer is fully self-contained.
    internal abstract class Iterator : IDisposable
    {
        public abstract RowBuffer RowBuffer { get; }

        public abstract void Open();
        public abstract void Dispose();
        public abstract bool Read();
    }
}
