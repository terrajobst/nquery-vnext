using System;

namespace NQuery.Iterators
{
    internal sealed class EmptyIterator : Iterator
    {
        private readonly RowBuffer _rowBuffer = new EmptyRowBuffer();

        public override RowBuffer RowBuffer
        {
            get { return _rowBuffer; }
        }

        public override void Open()
        {
        }

        public override bool Read()
        {
            return false;
        }
    }
}