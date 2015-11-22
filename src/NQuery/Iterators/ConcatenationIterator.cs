using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace NQuery.Iterators
{
    internal sealed class ConcatenationIterator : Iterator
    {
        private readonly ImmutableArray<Iterator> _inputs;
        private readonly ImmutableArray<ProjectedRowBuffer> _inputRowBuffers;
        private readonly IndirectedRowBuffer _rowBuffer;

        private int _currentInputIndex;
        private bool _currentInputIsOpen;

        public ConcatenationIterator(IEnumerable<Iterator> inputs, IEnumerable<ImmutableArray<RowBufferEntry>> entries)
        {
            _inputs = inputs.ToImmutableArray();
            _inputRowBuffers = entries.Select(e => new ProjectedRowBuffer(e)).ToImmutableArray();
            var rowBufferSize = _inputRowBuffers[0].Count;
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
                    _rowBuffer.ActiveRowBuffer = _inputRowBuffers[_currentInputIndex];
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