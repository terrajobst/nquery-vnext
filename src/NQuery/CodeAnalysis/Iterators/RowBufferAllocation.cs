using System.Collections.Frozen;
using System.Collections.Immutable;

using NQuery.CodeAnalysis.Algebra;

namespace NQuery.CodeAnalysis.Iterators;

internal sealed class RowBufferAllocation
{
    private readonly FrozenDictionary<ValueSlot, int> _mapping;

    public RowBufferAllocation(RowBufferAllocation? parent, RowBuffer rowBuffer, IEnumerable<ValueSlot> valueSlots)
    {
        Parent = parent;
        RowBuffer = rowBuffer;
        _mapping = GetValueMapping(valueSlots.ToImmutableArray());
    }

    private static FrozenDictionary<ValueSlot, int> GetValueMapping(ImmutableArray<ValueSlot> valueSlots)
    {
        var dictionary = new Dictionary<ValueSlot, int>(valueSlots.Length);
        for (var i = 0; i < valueSlots.Length; i++)
        {
            var outputValue = valueSlots[i];
            if (!dictionary.ContainsKey(outputValue))
                dictionary[outputValue] = i;
        }

        return dictionary.ToFrozenDictionary();
    }

    public RowBufferAllocation? Parent { get; }

    public RowBuffer RowBuffer { get; }

    public RowBufferEntry this[ValueSlot valueSlot]
    {
        get
        {
            return !_mapping.ContainsKey(valueSlot) && Parent is not null
                        ? Parent[valueSlot]
                        : new RowBufferEntry(RowBuffer, _mapping[valueSlot]);
        }
    }
}
