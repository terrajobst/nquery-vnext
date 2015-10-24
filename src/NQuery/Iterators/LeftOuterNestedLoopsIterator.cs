using System;
using System.Threading;

namespace NQuery.Iterators
{
    internal sealed class LeftOuterNestedLoopsIterator : NestedLoopsIterator
    {
        private readonly Iterator _left;
        private readonly Iterator _right;
        private readonly IteratorPredicate _predicate;
        private readonly IteratorPredicate _passthruPredicate;
        private readonly LeftOuterNestedLoopsRowBuffer _rowBuffer;

        private bool _bof;
        private bool _advanceOuter;
        private bool _outerRowHadMatchingInnerRow;

        public LeftOuterNestedLoopsIterator(Iterator left, Iterator right, IteratorPredicate predicate, IteratorPredicate passthruPredicate)
        {
            _left = left;
            _right = right;
            _predicate = predicate;
            _passthruPredicate = passthruPredicate;
            _rowBuffer = new LeftOuterNestedLoopsRowBuffer(_left.RowBuffer, _right.RowBuffer);
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
                    _outerRowHadMatchingInnerRow = false;

                    if (!_left.Read())
                        return false;

                    if (_passthruPredicate())
                    {
                        _rowBuffer.SetRightToNull();
                        _advanceOuter = true;
                        return true;
                    }

                    _right.Open();
                }

                // If we are bof or the inner is eof, reset the inner and
                // advance both cursors.
                if (_bof || !_right.Read())
                {
                    var shouldReturnRow = !_bof && !_outerRowHadMatchingInnerRow;

                    _bof = false;
                    _advanceOuter = true;

                    if (shouldReturnRow)
                    {
                        // We haven't returned the outer row yet since we couldn't find any matching inner 
                        // row. Set the values of the inner row to null and return the combined row.
                        _rowBuffer.SetRightToNull();
                        return true;
                    }

                    continue;
                }

                matchingRowFound = _predicate();
            }

            _rowBuffer.SetRight();
            _outerRowHadMatchingInnerRow = true;
            return true;
        }

        private class LeftOuterNestedLoopsRowBuffer : RowBuffer
        {
            private readonly RowBuffer _left;
            private readonly RowBuffer _right;
            private readonly IndirectedRowBuffer _indirectedRowBuffer;
            private readonly NullRowBuffer _rightNullRowBuffer;

            public LeftOuterNestedLoopsRowBuffer(RowBuffer left, RowBuffer right)
            {
                _left = left;
                _right = right;
                _indirectedRowBuffer = new IndirectedRowBuffer(_right.Count);
                _indirectedRowBuffer.ActiveRowBuffer = _right;
                _rightNullRowBuffer = new NullRowBuffer(right.Count);
            }

            public override int Count
            {
                get { return _left.Count + _right.Count; }
            }

            public void SetRight()
            {
                _indirectedRowBuffer.ActiveRowBuffer = _right;
            }

            public void SetRightToNull()
            {
                _indirectedRowBuffer.ActiveRowBuffer = _rightNullRowBuffer;
            }

            public override object this[int index]
            {
                get
                {
                    return index < _left.Count
                               ? _left[index]
                               : _indirectedRowBuffer[index - _left.Count];
                }
            }

            public override void CopyTo(object[] array, int destinationIndex)
            {
                _left.CopyTo(array, destinationIndex);
                _indirectedRowBuffer.CopyTo(array, _left.Count + destinationIndex);
            }
        }
    }
}