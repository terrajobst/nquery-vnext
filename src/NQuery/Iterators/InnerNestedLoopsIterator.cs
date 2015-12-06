using System;

namespace NQuery.Iterators
{
    internal sealed class InnerNestedLoopsIterator : NestedLoopsIterator
    {
        private readonly Iterator _left;
        private readonly Iterator _right;
        private readonly IteratorPredicate _predicate;
        private readonly IteratorPredicate _passthruPredicate;

        private bool _bof;
        private bool _advanceOuter;

        public InnerNestedLoopsIterator(Iterator left, Iterator right, IteratorPredicate predicate, IteratorPredicate passthruPredicate)
        {
            _left = left;
            _right = right;
            _predicate = predicate;
            _passthruPredicate = passthruPredicate;
            RowBuffer = new CombinedRowBuffer(left.RowBuffer, right.RowBuffer);
        }

        public override RowBuffer RowBuffer { get; }

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
            var matchingRowFound = false;
            while (!matchingRowFound)
            {
                if (_advanceOuter)
                {
                    _advanceOuter = false;

                    if (!_left.Read())
                        return false;

                    if (_passthruPredicate())
                    {
                        _advanceOuter = true;
                        return true;
                    }

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