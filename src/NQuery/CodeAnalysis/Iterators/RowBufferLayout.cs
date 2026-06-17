using System.Collections.Frozen;
using System.Collections.Immutable;

using NQuery.CodeAnalysis.Algebra;

namespace NQuery.CodeAnalysis.Iterators;

// The per-container layout of a row: walking an operator's output columns in order,
// each is classified and assigned the next free index in its container. The container
// counts size an ArrayRowBuffer; the per-column RowBufferColumn list is what slot maps
// and entry-based readers resolve against.
internal sealed class RowBufferLayout
{
    private RowBufferLayout(ImmutableArray<RowBufferColumn> columns, int objectCount, int bits32Count, int bits64Count, int bits128Count)
    {
        Columns = columns;
        ObjectCount = objectCount;
        Bits32Count = bits32Count;
        Bits64Count = bits64Count;
        Bits128Count = bits128Count;
    }

    public ImmutableArray<RowBufferColumn> Columns { get; }

    public int ObjectCount { get; }

    public int Bits32Count { get; }

    public int Bits64Count { get; }

    public int Bits128Count { get; }

    public static RowBufferLayout Create(IEnumerable<Type> columnTypes)
    {
        var columns = ImmutableArray.CreateBuilder<RowBufferColumn>();
        var objectCount = 0;
        var bits32Count = 0;
        var bits64Count = 0;
        var bits128Count = 0;

        foreach (var type in columnTypes)
        {
            var kind = RowBufferColumn.Classify(type);
            var index = kind switch
            {
                RowBufferColumnKind.Object => objectCount++,
                RowBufferColumnKind.Bits32 => bits32Count++,
                RowBufferColumnKind.Bits64 => bits64Count++,
                RowBufferColumnKind.Bits128 => bits128Count++,
                _ => throw ExceptionBuilder.UnexpectedValue(kind)
            };
            columns.Add(new RowBufferColumn(kind, index));
        }

        return new RowBufferLayout(columns.ToImmutable(), objectCount, bits32Count, bits64Count, bits128Count);
    }

    public static RowBufferLayout Create(ImmutableArray<ValueSlot> valueSlots)
    {
        return Create(valueSlots.Select(s => s.Type));
    }

    // Maps each value slot to its column. The layout is positional, so a slot that
    // appears more than once resolves to its first occurrence (matching how the
    // producing operator fills the buffer).
    public static FrozenDictionary<ValueSlot, RowBufferColumn> CreateSlotMap(ImmutableArray<ValueSlot> valueSlots)
    {
        var layout = Create(valueSlots);
        var map = new Dictionary<ValueSlot, RowBufferColumn>(valueSlots.Length);
        for (var i = 0; i < valueSlots.Length; i++)
        {
            if (!map.ContainsKey(valueSlots[i]))
                map.Add(valueSlots[i], layout.Columns[i]);
        }

        return map.ToFrozenDictionary();
    }
}
