using System;

namespace NQuery.Plan
{
    internal sealed class ArrayRowBuffer : RowBuffer
    {
        private readonly object[] _array;

        public ArrayRowBuffer(int size)
        {
            _array = new object[size];
        }

        public object[] Array
        {
            get { return _array; }
        }

        public override int Count
        {
            get { return _array.Length; }
        }

        public override object this[int index]
        {
            get { return _array[index]; }
        }
    }
}