using System;

namespace NQuery.Iterators
{
    internal sealed class IndirectedRowBuffer : RowBuffer
    {
        private readonly int _count;

        public IndirectedRowBuffer(int count)
        {
            _count = count;
        }

        public RowBuffer ActiveRowBuffer { get; set; }

        public override int Count
        {
            get { return _count; }
        }

        public override object this[int index]
        {
            get { return ActiveRowBuffer[index]; }
        }

        public override void CopyTo(object[] array, int destinationIndex)
        {
            ActiveRowBuffer.CopyTo(array, destinationIndex);
        }
    }
}