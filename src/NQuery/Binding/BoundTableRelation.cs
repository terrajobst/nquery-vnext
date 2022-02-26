using System.Collections.Immutable;

using NQuery.Symbols;

namespace NQuery.Binding
{
    internal sealed class BoundTableRelation : BoundRelation
    {
        public BoundTableRelation(TableInstanceSymbol tableInstance)
            : this(tableInstance, tableInstance.ColumnInstances)
        {
        }

        public BoundTableRelation(TableInstanceSymbol tableInstance, ImmutableArray<TableColumnInstanceSymbol> definedValues)
        {
            TableInstance = tableInstance;
            DefinedValues = definedValues;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.TableRelation; }
        }

        public TableInstanceSymbol TableInstance { get; }

        public ImmutableArray<TableColumnInstanceSymbol> DefinedValues { get; }

        public BoundTableRelation Update(TableInstanceSymbol tableInstance, ImmutableArray<TableColumnInstanceSymbol> definedValues)
        {
            if (tableInstance == TableInstance && definedValues == DefinedValues)
                return this;

            return new BoundTableRelation(tableInstance, definedValues);
        }

        public override IEnumerable<ValueSlot> GetDefinedValues()
        {
            return DefinedValues.Select(d => d.ValueSlot);
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