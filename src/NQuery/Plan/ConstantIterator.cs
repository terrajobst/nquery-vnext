using System;

namespace NQuery.Plan
{
    internal sealed class ConstantIterator : Iterator
    {
        private readonly EmptyRowBuffer _rowBuffer = new EmptyRowBuffer();
        private bool _isEof;

        public override RowBuffer RowBuffer
        {
            get { return _rowBuffer; }
        }

        public override void Open()
        {
            _isEof = false;
        }

        public override bool Read()
        {
            if (_isEof)
                return false;

            _isEof = true;
            return true;
        }
    }
}