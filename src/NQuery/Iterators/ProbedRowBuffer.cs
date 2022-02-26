namespace NQuery.Iterators
{
    internal sealed class ProbedRowBuffer : RowBuffer
    {
        private static readonly object BoxedTrue = true;
        private static readonly object BoxedFalse = false;

        private readonly RowBuffer _rowBuffer;
        private object _value;

        public ProbedRowBuffer(RowBuffer rowBuffer)
        {
            _rowBuffer = rowBuffer;
        }

        public void SetProbe(bool value)
        {
            _value = value ? BoxedTrue : BoxedFalse;
        }

        public override int Count
        {
            get { return _rowBuffer.Count + 1; }
        }

        public override object this[int index]
        {
            get
            {
                return index < _rowBuffer.Count
                    ? _rowBuffer[index]
                    : _value;
            }
        }

        public override void CopyTo(object[] array, int destinationIndex)
        {
            _rowBuffer.CopyTo(array, destinationIndex);
            array[_rowBuffer.Count + destinationIndex] = _value;
        }
    }
}