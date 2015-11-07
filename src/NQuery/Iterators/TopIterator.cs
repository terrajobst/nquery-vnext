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

        public override RowBuffer RowBuffer
        {
            get { return Input.RowBuffer; }
        }

        public override void Open()
        {
            Input.Open();
            _rowCount = 0;
        }

        public override bool Read()
        {
            if (!Input.Read())
                return false;

            if (_rowCount == Limit)
                return false;

            _rowCount++;
            return true;
        }
    }
}