using System;
using System.Collections.Generic;

namespace NQuery.Iterators
{
    internal sealed class ConcatenationIterator : Iterator
    {
        private readonly IList<Iterator> _inputs;
        private readonly IndirectedRowBuffer _rowBuffer;

        private int _currentInputIndex;
        private bool _currentInputIsOpen;

        public ConcatenationIterator(IList<Iterator> inputs, int rowBufferSize)
        {
            _inputs = inputs;
            _rowBuffer = new IndirectedRowBuffer(rowBufferSize);
        }

        public override RowBuffer RowBuffer
        {
            get { return _rowBuffer; }
        }

        public override void Open()
        {
            _currentInputIndex = 0;
            _currentInputIsOpen = false;
        }

        public override bool Read()
        {
            while (_currentInputIndex < _inputs.Count)
            {
                var currentInput = _inputs[_currentInputIndex];

                if (!_currentInputIsOpen)
                {
                    currentInput.Open();
                    _rowBuffer.ActiveRowBuffer = currentInput.RowBuffer;
                    _currentInputIsOpen = true;
                }

                if (currentInput.Read())
                    return true;

                _currentInputIndex++;
                _currentInputIsOpen = false;
            }

            return false;
        }

        private sealed class IndirectedRowBuffer : RowBuffer
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
        }
    }
}