#nullable enable

namespace NQuery.Iterators;

// One build-side row in a hash bucket. Rows sharing a key are chained through Next;
// Matched records whether the row was joined -- so an outer join can find the
// leftovers and a semi/anti/probing join can decide it from the build flush.
internal sealed class HashMatchEntry
{
    public object[] RowValues = null!;
    public HashMatchEntry? Next;
    public bool Matched;
}
