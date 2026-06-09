#nullable enable

using System.Collections.Frozen;
using System.Collections.Immutable;

using NQuery.Binding;
using NQuery.Symbols;

namespace NQuery.Algebra
{
    internal sealed class LogicalTableScan : LogicalOperator
    {
        public LogicalTableScan(TableInstanceSymbol tableInstance, ImmutableArray<TableColumnInstanceSymbol> definedValues)
        {
            TableInstance = tableInstance;
            DefinedValues = definedValues;
        }

        public override LogicalOperatorKind Kind => LogicalOperatorKind.TableScan;

        public TableInstanceSymbol TableInstance { get; }

        public ImmutableArray<TableColumnInstanceSymbol> DefinedValues { get; }

        protected override FrozenSet<ValueSlot> ComputeDefinedValueSlots() => DefinedValues.Select(d => d.ValueSlot).ToFrozenSet();

        protected override ImmutableArray<ValueSlot> ComputeOutputValueSlots() => DefinedValues.Select(d => d.ValueSlot).ToImmutableArray();
    }
}
