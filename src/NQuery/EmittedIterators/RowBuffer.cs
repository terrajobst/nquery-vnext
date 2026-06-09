#nullable enable

namespace NQuery.EmittedIterators
{
    internal abstract class RowBuffer
    {
        public abstract int Count { get; }
        public abstract object this[int index] { get; }
        public abstract void CopyTo(object[] array, int destinationIndex);
    }
}
