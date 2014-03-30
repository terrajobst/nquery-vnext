using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NQuery.Iterators
{
    internal sealed class ComputeScalarIterator : Iterator
    {
        private readonly Iterator _input;
        private readonly ImmutableArray<IteratorFunction> _definedValues;
        private readonly ArrayRowBuffer _rowBuffer;
        private readonly CombinedRowBuffer _combinedRowBuffer;

        public ComputeScalarIterator(Iterator input, IEnumerable<IteratorFunction> definedValues)
        {
            _input = input;
            _definedValues = definedValues.ToImmutableArray();
            _rowBuffer = new ArrayRowBuffer(_definedValues.Length);
            _combinedRowBuffer = new CombinedRowBuffer(input.RowBuffer, _rowBuffer);
        }

        public override RowBuffer RowBuffer
        {
            get { return _combinedRowBuffer; }
        }

        public override void Open()
        {
            _input.Open();
        }

        public override bool Read()
        {
            if (!_input.Read())
                return false;

            for (var i = 0; i < _definedValues.Length; i++)
                _rowBuffer.Array[i] = _definedValues[i](_input.RowBuffer);

            return true;
        }
    }
}