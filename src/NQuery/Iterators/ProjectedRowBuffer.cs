using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NQuery.Iterators
{
    internal sealed class ProjectedRowBuffer : RowBuffer
    {
        private readonly RowBuffer _source;
        private readonly ReadOnlyCollection<int> _projectedIndices;

        public ProjectedRowBuffer(RowBuffer source, IList<int> projectedIndices)
        {
            _source = source;
            _projectedIndices = new ReadOnlyCollection<int>(projectedIndices);
        }

        public override int Count
        {
            get { return _projectedIndices.Count; }
        }

        public override object this[int index]
        {
            get { return _source[_projectedIndices[index]]; }
        }
    }
}