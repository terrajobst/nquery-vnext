#nullable enable

using System.Collections.Frozen;
using System.Collections.Immutable;

using NQuery.Refactor.Binding;
using NQuery.Symbols;

namespace NQuery.Refactor.Planning
{
    internal sealed class PhysicalTableScan : PhysicalOperator
    {
        private readonly ImmutableArray<ValueSlot> _outputValueSlots;

        public PhysicalTableScan(TableInstanceSymbol tableInstance, ImmutableArray<TableColumnInstanceSymbol> definedValues, ImmutableArray<ValueSlot> outputValueSlots)
        {
            TableInstance = tableInstance;
            DefinedValues = definedValues;
            _outputValueSlots = outputValueSlots;
        }

        public override PhysicalOperatorKind Kind => PhysicalOperatorKind.TableScan;

        public TableInstanceSymbol TableInstance { get; }

        // Parallel to OutputValueSlots: DefinedValues[i] is read into OutputValueSlots[i].
        public ImmutableArray<TableColumnInstanceSymbol> DefinedValues { get; }

        protected override FrozenSet<ValueSlot> ComputeDefinedValueSlots() => _outputValueSlots.ToFrozenSet();

        protected override ImmutableArray<ValueSlot> ComputeOutputValueSlots() => _outputValueSlots;
    }
}
