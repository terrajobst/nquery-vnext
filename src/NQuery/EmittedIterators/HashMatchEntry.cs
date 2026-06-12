#nullable enable

namespace NQuery.EmittedIterators
{
    // One build-side row in a hash bucket. Rows sharing a key are chained through Next;
    // Matched records whether the row was joined (so outer joins can find the leftovers).
    internal sealed class HashMatchEntry
    {
        public object[] RowValues = null!;
        public HashMatchEntry? Next;
        public bool Matched;
    }
}
