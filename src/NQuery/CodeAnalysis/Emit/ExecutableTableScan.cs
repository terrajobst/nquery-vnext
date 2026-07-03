using System.Collections.Immutable;

using NQuery.CodeAnalysis.Algebra;
using NQuery.CodeAnalysis.Iterators;
using NQuery.Metadata;

namespace NQuery.CodeAnalysis.Emit;

internal sealed class ExecutableTableScan : ExecutableOperator
{
    private readonly TableDefinition _definition;
    private readonly RowBufferLayout _layout;
    private readonly Action<object, ArrayRowBuffer> _rowWriter;

    public ExecutableTableScan(ImmutableArray<ValueSlot> outputValueSlots, TableDefinition definition, RowBufferLayout layout, Action<object, ArrayRowBuffer> rowWriter)
        : base(outputValueSlots)
    {
        ThrowIfNull(definition);
        ThrowIfNull(layout);
        ThrowIfNull(rowWriter);

        _definition = definition;
        _layout = layout;
        _rowWriter = rowWriter;
    }

    public override Iterator CreateIterator(RecursiveWorkTableRegistry workTables, RowBuffer? outer) => new TableIterator(_definition, _layout, _rowWriter);
}
