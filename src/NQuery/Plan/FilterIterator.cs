using System;

namespace NQuery.Plan
{
    internal sealed class FilterIterator : Iterator
    {
        private readonly Iterator _input;
        private readonly Func<bool> _predicate;

        public FilterIterator(Iterator input, Func<bool> predicate)
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