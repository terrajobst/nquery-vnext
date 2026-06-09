#nullable enable

namespace NQuery.EmittedIterators
{
    // Like FilterIterator, but its predicate takes the row buffer as a parameter, so
    // the compiled predicate is shared across executions rather than bound to one.
    internal sealed class EmittedFilterIterator : Iterator
    {
        private readonly Iterator _input;
        private readonly EmittedPredicate _predicate;

        public EmittedFilterIterator(Iterator input, EmittedPredicate predicate)
        {
            _input = input;
            _predicate = predicate;
        }

        public override RowBuffer RowBuffer => _input.RowBuffer;

        public override void Open() => _input.Open();

        public override void Dispose() => _input.Dispose();

        public override bool Read()
        {
            while (_input.Read())
            {
                if (_predicate(_input.RowBuffer))
                    return true;
            }

            return false;
        }
    }
}
