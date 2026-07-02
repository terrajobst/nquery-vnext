using System.Collections.Immutable;

using NQuery.CodeAnalysis.Algebra;
using NQuery.CodeAnalysis.Iterators;

namespace NQuery.CodeAnalysis.Emit;

// The recursion driver: anchor rows seed the working table, then the recursive
// members are re-executed per round against it (see RecursionIterator). Like the
// concatenation, each input is projected into the unified output order via its
// defined-value slots; InputValueSlots[0] is the anchor's, the members follow.
// The work table shared with the reference leaves is resolved by recursion token
// from the per-execution registry (see RecursiveWorkTableRegistry).
internal sealed class ExecutableRecursiveUnion : ExecutableOperator
{
    private readonly RecursionToken _token;
    private readonly ExecutableOperator _anchor;
    private readonly ImmutableArray<ExecutableOperator> _members;
    private readonly ImmutableArray<LogicalUnifiedValue> _definedValues;

    public ExecutableRecursiveUnion(ImmutableArray<ValueSlot> outputValueSlots, RecursionToken token, ExecutableOperator anchor, ImmutableArray<ExecutableOperator> members, ImmutableArray<LogicalUnifiedValue> definedValues)
        : base(outputValueSlots)
    {
        _token = token;
        _anchor = anchor;
        _members = members;
        _definedValues = definedValues;
    }

    public override Iterator CreateIterator(RecursiveWorkTableRegistry workTables, RowBuffer? outer)
    {
        var workTable = workTables.GetOrCreate(_token);

        var anchor = _anchor.CreateIterator(workTables, outer);
        var members = _members.Select(m => m.CreateIterator(workTables, outer)).ToImmutableArray();

        var anchorAllocation = Allocate(_anchor, anchor);
        var anchorInput = new ProjectedRowBuffer(_definedValues.Select(d => anchorAllocation[d.InputValueSlots[0]]));

        var memberInputs = members
            .Select((iterator, i) =>
            {
                var allocation = Allocate(_members[i], iterator);
                return (RowBuffer)new ProjectedRowBuffer(_definedValues.Select(d => allocation[d.InputValueSlots[i + 1]]));
            })
            .ToImmutableArray();

        return new RecursionIterator(anchor, anchorInput, members, memberInputs, workTable);
    }
}
