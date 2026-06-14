using System.Collections.Frozen;
using System.Collections.Immutable;

using NQuery.Symbols;

namespace NQuery.Algebra;

internal sealed class LogicalTableScan : LogicalOperator
{
    private readonly ImmutableArray<ValueSlot> _outputValueSlots;

    public LogicalTableScan(TableInstanceSymbol tableInstance, ImmutableArray<TableColumnInstanceSymbol> definedValues, ImmutableArray<ValueSlot> outputValueSlots)
    {
        TableInstance = tableInstance;
        DefinedValues = definedValues;
        _outputValueSlots = outputValueSlots;
    }

    public override LogicalOperatorKind Kind => LogicalOperatorKind.TableScan;

    public TableInstanceSymbol TableInstance { get; }

    // The scanned column instances, parallel to the output slots: DefinedValues[i] is read into
    // OutputValueSlots[i]. The slots are minted by the algebrizer -- the symbols no longer
    // carry one.
    public ImmutableArray<TableColumnInstanceSymbol> DefinedValues { get; }

    protected override FrozenSet<ValueSlot> ComputeDefinedValueSlots() => _outputValueSlots.ToFrozenSet();

    protected override ImmutableArray<ValueSlot> ComputeOutputValueSlots() => _outputValueSlots;
}
