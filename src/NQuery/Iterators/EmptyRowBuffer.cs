using System;

namespace NQuery.Iterators
{
    internal sealed class EmptyRowBuffer : RowBuffer
    {
        public override int Count
        {
            get { return 0; }
        }

        public override object this[int index]
        {
            get { throw new ArgumentOutOfRangeException(); }
        }

        public override void CopyTo(object[] array, int destinationIndex)
        {
        }
    }
}