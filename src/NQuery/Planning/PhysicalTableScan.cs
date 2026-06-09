#nullable enable

using System.Collections.Frozen;
using System.Collections.Immutable;

using NQuery.Binding;
using NQuery.Symbols;

namespace NQuery.Planning
{
    internal sealed class PhysicalTableScan : PhysicalOperator
    {
        public PhysicalTableScan(TableInstanceSymbol tableInstance, ImmutableArray<TableColumnInstanceSymbol> definedValues)
        {
            TableInstance = tableInstance;
            DefinedValues = definedValues;
        }

        public override PhysicalOperatorKind Kind => PhysicalOperatorKind.TableScan;

        public TableInstanceSymbol TableInstance { get; }

        public ImmutableArray<TableColumnInstanceSymbol> DefinedValues { get; }

        protected override FrozenSet<ValueSlot> ComputeDefinedValueSlots() => DefinedValues.Select(d => d.ValueSlot).ToFrozenSet();

        protected override ImmutableArray<ValueSlot> ComputeOutputValueSlots() => DefinedValues.Select(d => d.ValueSlot).ToImmutableArray();
    }
}
