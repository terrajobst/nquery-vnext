using System.Collections.Frozen;
using System.Collections.Immutable;

using NQuery.CodeAnalysis.Algebra;

namespace NQuery.CodeAnalysis.Iterators;

internal sealed class RowBufferAllocation
{
    private readonly FrozenDictionary<ValueSlot, RowBufferColumn> _mapping;

    public RowBufferAllocation(RowBufferAllocation? parent, RowBuffer rowBuffer, IEnumerable<ValueSlot> valueSlots)
    {
        ThrowIfNull(rowBuffer);
        ThrowIfNull(valueSlots);

        Parent = parent;
        RowBuffer = rowBuffer;
        _mapping = RowBufferLayout.CreateSlotMap([.. valueSlots]);
    }

    public RowBufferAllocation? Parent { get; }

    public RowBuffer RowBuffer { get; }

    public RowBufferEntry this[ValueSlot valueSlot]
    {
        get
        {
            return !_mapping.ContainsKey(valueSlot) && Parent is not null
                        ? Parent[valueSlot]
                        : new RowBufferEntry(RowBuffer, _mapping[valueSlot], valueSlot.Type);
        }
    }
}
