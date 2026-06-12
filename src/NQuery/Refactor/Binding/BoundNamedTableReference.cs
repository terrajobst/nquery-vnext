using System.Collections.Immutable;

using NQuery.Symbols;

namespace NQuery.Refactor.Binding
{
    internal sealed class BoundNamedTableReference : BoundTableReference
    {
        public BoundNamedTableReference(TableInstanceSymbol tableInstance)
            : this(tableInstance, tableInstance.ColumnInstances)
        {
        }

        public BoundNamedTableReference(TableInstanceSymbol tableInstance, ImmutableArray<TableColumnInstanceSymbol> definedValues)
        {
            TableInstance = tableInstance;
            DefinedValues = definedValues;
        }

        public override BoundNodeKind Kind => BoundNodeKind.NamedTableReference;

        public TableInstanceSymbol TableInstance { get; }

        public ImmutableArray<TableColumnInstanceSymbol> DefinedValues { get; }

        public override IEnumerable<ValueSlot> GetDefinedValues()
        {
            return DefinedValues.Select(d => d.ValueSlotRefactor);
        }

        public override IEnumerable<ValueSlot> GetOutputValues()
        {
            return GetDefinedValues();
        }

        public override string ToString()
        {
            return TableInstance.Name;
        }
    }
}
