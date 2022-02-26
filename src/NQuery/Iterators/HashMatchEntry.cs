namespace NQuery.Iterators
{
    internal sealed class HashMatchEntry
    {
        public object[] RowValues;
        public HashMatchEntry Next;
        public bool Matched;
    }
}