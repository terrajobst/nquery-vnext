using System;

namespace NQuery.Iterators
{
    internal sealed class FilterIterator : Iterator
    {
        private readonly Iterator _input;
        private readonly IteratorPredicate _predicate;

        public FilterIterator(Iterator input, IteratorPredicate predicate)
        {
            _input = input;
            _predicate = predicate;
        }

        public override RowBuffer RowBuffer
        {
            get { return _input.RowBuffer; }
        }

        public override void Open()
        {
            _input.Open();
        }

        public override void Dispose()
        {
            _input.Dispose();
        }

        public override bool Read()
        {
            var predicateIsTrue = false;
            while (!predicateIsTrue)
            {
                if (!_input.Read())
                    break;

                predicateIsTrue = _predicate();
            }

            return predicateIsTrue;
        }
    }
}