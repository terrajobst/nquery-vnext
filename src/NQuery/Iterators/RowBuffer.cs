using System;

namespace NQuery.Iterators
{
    internal abstract class RowBuffer
    {
        public abstract int Count { get; }
        public abstract object this[int index] { get; }
    }
}