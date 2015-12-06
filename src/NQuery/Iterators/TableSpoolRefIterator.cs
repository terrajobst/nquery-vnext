using System;

namespace NQuery.Iterators
{
    internal sealed class TableSpoolRefIterator : Iterator
    {
        private readonly TableSpoolStack _tableSpoolStack;
        private readonly IndirectedRowBuffer _rowBuffer;

        public TableSpoolRefIterator(TableSpoolStack tableSpoolStack)
        {
            _tableSpoolStack = tableSpoolStack;
            _rowBuffer = new IndirectedRowBuffer(_tableSpoolStack.RowBufferCount);
        }

        public override RowBuffer RowBuffer
        {
            get { return _rowBuffer; }
        }

        public override void Open()
        {
        }

        public override void Dispose()
        {
        }

        public override bool Read()
        {
            if (_tableSpoolStack.IsEmpty)
                return false;

            var spoolEntry = _tableSpoolStack.Pop();
            _rowBuffer.ActiveRowBuffer = spoolEntry;
            return true;
        }
    }
}