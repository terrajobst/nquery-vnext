using System;

namespace NQuery.Iterators
{
    internal sealed class LeftAntiSemiNestedLoopsIterator : NestedLoopsIterator
    {
        private readonly Iterator _left;
        private readonly Iterator _right;
        private readonly IteratorPredicate _predicate;
        private readonly IteratorPredicate _passthruPredicate;

        private bool _bof;
        private bool _advanceOuter;

        public LeftAntiSemiNestedLoopsIterator(Iterator left, Iterator right, IteratorPredicate predicate, IteratorPredicate passthruPredicate)
        {
            _left = left;
            _right = right;
            _predicate = predicate;
            _passthruPredicate = passthruPredicate;
        }

        public override RowBuffer RowBuffer
        {
            get { return _left.RowBuffer; }
        }

        public override void Open()
        {
            _left.Open();
            _advanceOuter = false;
            _bof = true;
        }

        public override bool Read()
        {
            while (true)
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

                if (_bof)
                {
                    _bof = false;
                    _advanceOuter = true;
                    continue;
                }

                if (!_right.Read())
                {
                    _advanceOuter = true;
                    return true;
                }

                // Check predicate.
                var matchingRowFound = _predicate();
                if (matchingRowFound)
                    _advanceOuter = true;
            }
        }
    }
}