namespace NQuery.CodeAnalysis.Iterators;

// One build-side row in a hash bucket. Rows sharing a key are chained through Next;
// Matched records whether the row was joined -- so an outer join can find the
// leftovers and a semi/anti/probing join can decide it from the build flush. Row is a
// materialized clone of the build row (see RowBuffer.Clone), so it outlives the cursor.
internal sealed class HashMatchEntry
{
    public ArrayRowBuffer Row { get; set; } = null!;
    public HashMatchEntry? Next { get; set; }
    public bool Matched { get; set; }
}
