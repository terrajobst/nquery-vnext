#nullable enable

namespace NQuery.Iterators;

// Exposes the current build-side HashMatchEntry's stored row as a RowBuffer, so the
// hash table's materialized rows can flow through the combined output buffer.
internal sealed class HashMatchEntryRowBuffer : RowBuffer
{
    public HashMatchEntry? Entry { get; set; }

    public override int Count => Entry!.RowValues.Length;

    public override object this[int index] => Entry!.RowValues[index];

    public override void CopyTo(object[] array, int destinationIndex)
    {
        var source = Entry!.RowValues;
        System.Array.Copy(source, 0, array, destinationIndex, source.Length);
    }
}
