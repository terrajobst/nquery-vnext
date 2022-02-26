namespace NQuery.Iterators
{
    internal sealed class IndirectedRowBuffer : RowBuffer
    {
        public IndirectedRowBuffer(int count)
        {
            Count = count;
        }

        public RowBuffer ActiveRowBuffer { get; set; }

        public override int Count { get; }

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