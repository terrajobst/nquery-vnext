using System;

using NQuery.Iterators;

namespace NQuery.Tests.Iterators
{
    internal sealed class MockedRowBuffer : RowBuffer
    {
        public MockedRowBuffer(int count)
        {
            Count = count;
            Value = new object[count];
        }

        public MockedRowBuffer(object[] value)
        {
            Count = value.Length;
            Value = value;
        }

        public override int Count { get; }

        public object[] Value { get; set; }

        public override object this[int index]
        {
            get
            {
                if (index < 0 || Count <= index)
                    throw new IndexOutOfRangeException();

                return Value[index];
            }
        }

        public override void CopyTo(object[] array, int destinationIndex)
        {
            Value.CopyTo(array, destinationIndex);
        }
    }
}