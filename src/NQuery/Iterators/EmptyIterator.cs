namespace NQuery.Iterators
{
    internal sealed class EmptyIterator : Iterator
    {
        public override RowBuffer RowBuffer { get; } = new EmptyRowBuffer();

        public override void Open()
        {
        }

        public override void Dispose()
        {
        }

        public override bool Read()
        {
            return false;
        }
    }
}