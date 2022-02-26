namespace NQuery.Iterators
{
    internal sealed class TableSpoolIterator : Iterator
    {
        private readonly Iterator _input;
        private readonly TableSpoolStack _tableSpoolStack;

        public TableSpoolIterator(Iterator input, TableSpoolStack tableSpoolStack)
        {
            _input = input;
            _tableSpoolStack = tableSpoolStack;
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

            // Read row from input and store it.
            var spoolEntry = new ArrayRowBuffer(_input.RowBuffer.Count);
            _input.RowBuffer.CopyTo(spoolEntry.Array, 0);
            _tableSpoolStack.Push(spoolEntry);

            return true;
        }
    }
}