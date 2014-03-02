using System;

namespace NQuery.Iterators
{
    internal abstract class Iterator : IDisposable
    {
        public virtual void Dispose()
        {
        }

        public virtual void Initialize()
        {
        }

        public abstract RowBuffer RowBuffer { get; }
        public abstract void Open();
        public abstract bool Read();
    }
}