using System.Collections.Immutable;

using NQuery.CodeAnalysis.Algebra;
using NQuery.CodeAnalysis.Iterators;
using NQuery.Metadata;

namespace NQuery.CodeAnalysis.Emit;

internal sealed class ExecutableTableScan : ExecutableOperator
{
    private readonly TableDefinition _definition;
    private readonly RowBufferLayout _layout;
    private readonly ImmutableArray<Action<object, ArrayRowBuffer>> _columnWriters;

    public ExecutableTableScan(ImmutableArray<ValueSlot> outputValueSlots, TableDefinition definition, RowBufferLayout layout, ImmutableArray<Action<object, ArrayRowBuffer>> columnWriters)
        : base(outputValueSlots)
    {
        _definition = definition;
        _layout = layout;
        _columnWriters = columnWriters;
    }

    public override Iterator CreateIterator(RowBuffer? outer) => new TableIterator(_definition, _layout, _columnWriters);
}
