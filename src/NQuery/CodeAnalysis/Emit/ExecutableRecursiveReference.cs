using System.Collections.Immutable;

using NQuery.CodeAnalysis.Algebra;
using NQuery.CodeAnalysis.Iterators;

namespace NQuery.CodeAnalysis.Emit;

// The working-table scan inside a recursive member. Its layout is the reference's
// own output slots, which are positional (and type-identical) to the enclosing
// union's unified columns -- the shape of the rows the work table spools. The
// work table itself is resolved by recursion token from the per-execution
// registry, so the leaf and its union connect without either creating the other.
internal sealed class ExecutableRecursiveReference : ExecutableOperator
{
    private readonly RecursionToken _token;
    private readonly RowBufferLayout _layout;

    public ExecutableRecursiveReference(ImmutableArray<ValueSlot> outputValueSlots, RecursionToken token, RowBufferLayout layout)
        : base(outputValueSlots)
    {
        _token = token;
        _layout = layout;
    }

    public override Iterator CreateIterator(RecursiveWorkTableRegistry workTables, RowBuffer? outer)
    {
        var workTable = workTables.GetOrCreate(_token);
        return new RecursiveReferenceIterator(workTable, _layout.ObjectCount, _layout.Bits32Count, _layout.Bits64Count, _layout.Bits128Count);
    }
}
