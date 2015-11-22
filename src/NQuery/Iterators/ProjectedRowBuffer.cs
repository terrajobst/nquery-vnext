using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NQuery.Iterators
{
    internal sealed class ProjectedRowBuffer : RowBuffer
    {
        private readonly ImmutableArray<RowBufferEntry> _projectedIndices;

        public ProjectedRowBuffer(IEnumerable<RowBufferEntry> projectedEntries)
        {
            _projectedIndices = projectedEntries.ToImmutableArray();
        }

        public override int Count
        {
            get { return _projectedIndices.Length; }
        }

        public override object this[int index]
        {
            get { return _projectedIndices[index].GetValue(); }
        }

        public override void CopyTo(object[] array, int destinationIndex)
        {
            for (var i = 0; i < _projectedIndices.Length; i++)
                array[i] = _projectedIndices[i].GetValue();
        }
    }
}