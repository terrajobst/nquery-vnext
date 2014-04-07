using System;

namespace NQuery.Iterators
{
    internal sealed class InnerNestedLoopsIterator : Iterator
    {
        private readonly Iterator _left;
        private readonly Iterator _right;
        private readonly IteratorPredicate _predicate;
        private readonly RowBuffer _rowBuffer;

        private bool _bof;
        private bool _advanceOuter;

        public InnerNestedLoopsIterator(Iterator left, Iterator right, IteratorPredicate predicate)
        {
            _left = left;
            _right = right;
            _predicate = predicate;
            _rowBuffer = new CombinedRowBuffer(left.RowBuffer, right.RowBuffer);
        }

        public override RowBuffer RowBuffer
        {
            get { return _rowBuffer; }
        }

        public override void Open()
        {
            _left.Open();
            _advanceOuter = false;
            _bof = true;
        }

        public override bool Read()
        {
            var matchingRowFound = false;
            while (!matchingRowFound)
            {
                if (_advanceOuter)
                {
                    _advanceOuter = false;

                    if (!_left.Read())
                        return false;

                    _right.Open();
                }

                // If we are bof or the inner is eof, reset the inner and
                // advance both cursors.

                if (_bof || !_right.Read())
                {
                    _bof = false;
                    _advanceOuter = true;
                    continue;
                }

                // Check predicate.
                matchingRowFound = _predicate();
            }

            return true;
        }
    }
}