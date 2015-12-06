using System;

namespace NQuery.Iterators
{
    internal sealed class ProbingLeftSemiNestedLoopsIterator : NestedLoopsIterator
    {
        private readonly Iterator _left;
        private readonly Iterator _right;
        private readonly IteratorPredicate _predicate;
        private readonly ProbedRowBuffer _rowBuffer;

        private bool _bof;
        private bool _advanceOuter;

        public ProbingLeftSemiNestedLoopsIterator(Iterator left, Iterator right, IteratorPredicate predicate)
        {
            _left = left;
            _right = right;
            _predicate = predicate;
            _rowBuffer = new ProbedRowBuffer(left.RowBuffer);
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

        public override void Dispose()
        {
            _left.Dispose();
            _right.Dispose();
        }

        public override bool Read()
        {
            _rowBuffer.SetProbe(false);
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

                if (_bof)
                {
                    _bof = false;
                    _advanceOuter = true;
                    continue;
                }

                // If the inner is eof, reset the inner and advance both cursors.
                if (!_right.Read())
                {
                    _advanceOuter = true;
                    // We found no matching row. However, since this a probing iterator
                    // we must return this row as well.
                    return true;
                }

                // Check predicate.
                matchingRowFound = _predicate();

                if (matchingRowFound)
                    _advanceOuter = true;
            }

            _rowBuffer.SetProbe(true);
            return true;
        }
    }
}