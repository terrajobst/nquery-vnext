using System.Collections.Immutable;

using NQuery.CodeAnalysis.Algebra;
using NQuery.CodeAnalysis.Iterators;

namespace NQuery.CodeAnalysis.Emit;

// The lazy index spool (see PhysicalIndexSpool): its index key resolves against the
// input's row buffer, its probe key against the outer row buffer an enclosing apply
// supplies -- outerSlots is that buffer's layout, exactly as correlated filters use
// it.
internal sealed class ExecutableIndexSpool : ExecutableOperator
{
    private readonly ExecutableOperator _input;
    private readonly ValueSlot _indexKey;
    private readonly ValueSlot _probeKey;
    private readonly ImmutableArray<ValueSlot> _outerSlots;

    public ExecutableIndexSpool(ImmutableArray<ValueSlot> outputValueSlots, ExecutableOperator input, ValueSlot indexKey, ValueSlot probeKey, ImmutableArray<ValueSlot> outerSlots)
        : base(outputValueSlots)
    {
        ThrowIfNull(input);
        ThrowIfNull(indexKey);
        ThrowIfNull(probeKey);

        _input = input;
        _indexKey = indexKey;
        _probeKey = probeKey;
        _outerSlots = outerSlots;
    }

    public override Iterator CreateIterator(RecursiveWorkTableRegistry workTables, RowBuffer? outer)
    {
        // The planner only chooses a spool under an apply, whose iterator always
        // supplies the outer row buffer.
        if (outer is null)
            throw new InvalidOperationException("An index spool requires the outer row of an enclosing apply, but none was supplied.");

        var input = _input.CreateIterator(workTables, outer);
        var indexEntry = Allocate(_input, input)[_indexKey];
        var probeEntry = new RowBufferAllocation(null, outer, _outerSlots)[_probeKey];
        return new IndexSpoolIterator(input, indexEntry, probeEntry);
    }
}
