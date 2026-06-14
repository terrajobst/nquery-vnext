#nullable enable

namespace NQuery.Iterators;

// A buffer whose backing buffer can be swapped at run time. Outer joins use it to
// flip the right side between the real row and an all-NULL row.
internal sealed class IndirectedRowBuffer : RowBuffer
{
    public IndirectedRowBuffer(int count, RowBuffer activeRowBuffer)
    {
        Count = count;
        ActiveRowBuffer = activeRowBuffer;
    }

    public RowBuffer ActiveRowBuffer { get; set; }

    public override int Count { get; }

    public override object this[int index]
    {
        get { return ActiveRowBuffer[index]; }
    }

    public override void CopyTo(object[] array, int destinationIndex)
    {
        ActiveRowBuffer.CopyTo(array, destinationIndex);
    }
}
