using System;
using System.Collections.Generic;

namespace NQuery.Plan
{
    internal sealed class ProjectionIterator : Iterator
    {
        private readonly Iterator _input;
        private readonly ProjectedRowBuffer _rowBuffer;

        public ProjectionIterator(Iterator input, IList<int> projectedIndices)
        {
            _input = input;
            _rowBuffer = new ProjectedRowBuffer(input.RowBuffer, projectedIndices);
        }

        public override RowBuffer RowBuffer
        {
            get { return _rowBuffer; }
        }

        public override void Open()
        {
            _input.Open();
        }

        public override bool Read()
        {
            return _input.Read();
        }
    }
}