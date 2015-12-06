using System;

namespace NQuery.Iterators
{
    internal sealed class AssertIterator : Iterator
    {
        private readonly Iterator _input;
        private readonly IteratorPredicate _predicate;
        private readonly string _message;

        public AssertIterator(Iterator input, IteratorPredicate predicate, string message)
        {
            _input = input;
            _predicate = predicate;
            _message = message;
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
            if (!_input.Read())
                return false;

            if (!_predicate())
                throw new InvalidOperationException(_message);

            return true;
        }
    }
}