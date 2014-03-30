using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NQuery.Iterators
{
    internal sealed class ProjectedRowBuffer : RowBuffer
    {
        private readonly RowBuffer _source;
        private readonly ImmutableArray<int> _projectedIndices;

        public ProjectedRowBuffer(RowBuffer source, IEnumerable<int> projectedIndices)
        {
            _source = source;
            _projectedIndices = projectedIndices.ToImmutableArray();
        }

        public override int Count
        {
            get { return _projectedIndices.Length; }
        }

        public override object this[int index]
        {
            get { return _source[_projectedIndices[index]]; }
        }

        public override void CopyTo(object[] array, int destinationIndex)
        {
            for (var i = 0; i < _projectedIndices.Length; i++)
                array[i] = _source[_projectedIndices[i]];
        }
    }
}