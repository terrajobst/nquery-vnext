namespace NQuery.Iterators;

// The runtime cursor of the pipeline. It is intentionally a separate type
// so the emitted layer is fully self-contained.
internal abstract class Iterator : IDisposable
{
    public abstract RowBuffer RowBuffer { get; }

    public abstract void Open();
    public abstract void Dispose();
    public abstract bool Read();
}
