using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NQuery.Iterators
{
    internal sealed class ConcatenationIterator : Iterator
    {
        private readonly ImmutableArray<Iterator> _inputs;
        private readonly IndirectedRowBuffer _rowBuffer;

        private int _currentInputIndex;
        private bool _currentInputIsOpen;

        public ConcatenationIterator(IEnumerable<Iterator> inputs, int rowBufferSize)
        {
            _inputs = inputs.ToImmutableArray();
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
            while (_currentInputIndex < _inputs.Length)
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
    }
}