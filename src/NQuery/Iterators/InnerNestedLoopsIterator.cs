#nullable enable

namespace NQuery.Iterators
{
    internal sealed class InnerNestedLoopsIterator : NestedLoopsIterator
    {
        private readonly Iterator _left;
        private readonly Iterator _right;
        private readonly EmittedPredicate _predicate;
        private readonly EmittedPredicate _passthruPredicate;
        private readonly CombinedRowBuffer _rowBuffer;
        private readonly RowBuffer _predicateRowBuffer;

        private bool _bof;
        private bool _advanceOuter;

        public InnerNestedLoopsIterator(Iterator left, Iterator right, EmittedPredicate predicate, EmittedPredicate passthruPredicate, RowBuffer? outer = null)
        {
            _left = left;
            _right = right;
            _predicate = predicate;
            _passthruPredicate = passthruPredicate;
            _rowBuffer = new CombinedRowBuffer(left.RowBuffer, right.RowBuffer);

            // When this join is the correlated part of an Apply's right side, its predicate
            // references the outer (left) row. The outer buffer is then prepended to the
            // (left ++ right) buffer, matching the (outer ++ left ++ right) slot layout the
            // predicate was compiled against; the rows this iterator exposes are unchanged.
            _predicateRowBuffer = outer is null ? _rowBuffer : new CombinedRowBuffer(outer, _rowBuffer);
        }

        public override RowBuffer RowBuffer => _rowBuffer;

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

                    if (_passthruPredicate(_predicateRowBuffer))
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
                matchingRowFound = _predicate(_predicateRowBuffer);
            }

            return true;
        }
    }
}
