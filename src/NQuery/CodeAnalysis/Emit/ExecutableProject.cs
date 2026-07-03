using System.Collections.Immutable;

using NQuery.CodeAnalysis.Algebra;
using NQuery.CodeAnalysis.Iterators;

namespace NQuery.CodeAnalysis.Emit;

internal sealed class ExecutableProject : ExecutableOperator
{
    private readonly ExecutableOperator _input;
    private readonly ImmutableArray<ValueSlot> _outputs;

    public ExecutableProject(ImmutableArray<ValueSlot> outputValueSlots, ExecutableOperator input, ImmutableArray<ValueSlot> outputs)
        : base(outputValueSlots)
    {
        ThrowIfNull(input);

        _input = input;
        _outputs = outputs;
    }

    public override Iterator CreateIterator(RecursiveWorkTableRegistry workTables, RowBuffer? outer)
    {
        var input = _input.CreateIterator(workTables, outer);
        var allocation = Allocate(_input, input);
        var entries = _outputs.Select(s => allocation[s]);
        return new ProjectionIterator(input, entries);
    }
}
