using System;

namespace NQuery.Iterators
{
    internal sealed class NullRowBuffer : RowBuffer
    {
        private readonly int _count;

        public NullRowBuffer(int count)
        {
            _count = count;
        }

        public override int Count
        {
            get { return _count; }
        }

        public override object this[int index]
        {
            get { return null; }
        }

        public override void CopyTo(object[] array, int destinationIndex)
        {
            Array.Clear(array, destinationIndex, _count);
        }
    }
}