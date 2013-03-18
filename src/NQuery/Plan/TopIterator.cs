using System;

namespace NQuery.Plan
{
    internal class TopIterator : Iterator
    {
        private readonly Iterator _input;
        private readonly int _limit;

        private int _rowCount;

        public TopIterator(Iterator input, int limit)
        {
            _input = input;
            _limit = limit;
        }

        protected Iterator Input
        {
            get { return _input; }
        }

        protected int Limit
        {
            get { return _limit; }
        }

        public override RowBuffer RowBuffer
        {
            get { return _input.RowBuffer; }
        }

        public override void Open()
        {
            _input.Open();
            _rowCount = 0;
        }

        public override bool Read()
        {
            if (!_input.Read())
                return false;

            if (_rowCount == _limit)
                return false;

            _rowCount++;
            return true;
        }
    }
}