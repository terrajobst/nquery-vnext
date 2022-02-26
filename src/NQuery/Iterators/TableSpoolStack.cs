namespace NQuery.Iterators
{
    internal sealed class TableSpoolStack
    {
        private readonly int _rowBufferCount;
        private readonly Stack<RowBuffer> _stack = new Stack<RowBuffer>();

        public TableSpoolStack(int rowBufferCount)
        {
            _rowBufferCount = rowBufferCount;
        }

        public int RowBufferCount
        {
            get { return _rowBufferCount; }
        }

        public bool IsEmpty
        {
            get { return _stack.Count == 0; }
        }

        public void Push(RowBuffer rowBuffer)
        {
            _stack.Push(rowBuffer);
        }

        public RowBuffer Pop()
        {
            return _stack.Pop();
        }
    }
}