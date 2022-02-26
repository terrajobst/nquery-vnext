namespace NQuery.Iterators
{
    internal sealed class HashMatchEntryRowBuffer : RowBuffer
    {
        public HashMatchEntry Entry { get; set; }

        public override int Count
        {
            get { return Entry.RowValues.Length; }
        }

        public override object this[int index]
        {
            get { return Entry.RowValues[index]; }
        }

        public override void CopyTo(object[] array, int destinationIndex)
        {
            var source = Entry.RowValues;
            Array.Copy(source, 0, array, destinationIndex, source.Length);
        }
    }
}