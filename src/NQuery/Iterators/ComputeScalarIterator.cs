using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NQuery.Iterators
{
    internal sealed class ComputeScalarIterator : Iterator
    {
        private readonly Iterator _input;
        private readonly ReadOnlyCollection<Func<object>> _definedValues;
        private readonly ArrayRowBuffer _rowBuffer;
        private readonly CombinedRowBuffer _combinedRowBuffer;

        public ComputeScalarIterator(Iterator input, IList<Func<object>> definedValues)
        {
            _input = input;
            _definedValues = new ReadOnlyCollection<Func<object>>(definedValues);
            _rowBuffer = new ArrayRowBuffer(definedValues.Count);
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

            for (var i = 0; i < _definedValues.Count; i++)
                _rowBuffer.Array[i] = _definedValues[i]();

            return true;
        }
    }
}