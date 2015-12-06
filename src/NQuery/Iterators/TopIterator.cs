using System;

namespace NQuery.Iterators
{
    internal class TopIterator : Iterator
    {
        private int _rowCount;

        public TopIterator(Iterator input, int limit)
        {
            Input = input;
            Limit = limit;
        }

        protected Iterator Input { get; }

        protected int Limit { get; }

        protected bool InputExhausted { get; private set; }

        public override RowBuffer RowBuffer => Input.RowBuffer;

        public override void Open()
        {
            Input.Open();
            InputExhausted = false;
            _rowCount = 0;
        }

        public override void Dispose()
        {
            Input.Dispose();
        }

        public override bool Read()
        {
            if (!Input.Read())
            {
                InputExhausted = true;
                return false;
            }

            if (_rowCount == Limit)
                return false;

            _rowCount++;
            return true;
        }
    }
}