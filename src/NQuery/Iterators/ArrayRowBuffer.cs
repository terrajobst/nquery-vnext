namespace NQuery.Iterators
{
    internal sealed class ArrayRowBuffer : RowBuffer
    {
        public ArrayRowBuffer(int size)
        {
            Array = new object[size];
        }

        public object[] Array { get; }

        public override int Count
        {
            get { return Array.Length; }
        }

        public override object this[int index]
        {
            get { return Array[index]; }
        }

        public override void CopyTo(object[] array, int destinationIndex)
        {
            System.Array.Copy(Array, 0, array, destinationIndex, Array.Length);
        }
    }
}