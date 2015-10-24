using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using NQuery.Symbols;

namespace NQuery.Binding
{
    internal sealed class BoundTableRelation : BoundRelation
    {
        private readonly TableInstanceSymbol _tableInstance;
        private readonly ImmutableArray<TableColumnInstanceSymbol> _definedValues;

        public BoundTableRelation(TableInstanceSymbol tableInstance)
            : this(tableInstance, tableInstance.ColumnInstances)
        {
        }

        public BoundTableRelation(TableInstanceSymbol tableInstance, ImmutableArray<TableColumnInstanceSymbol> definedValues)
        {
            _tableInstance = tableInstance;
            _definedValues = definedValues;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.TableRelation; }
        }

        public TableInstanceSymbol TableInstance
        {
            get { return _tableInstance; }
        }

        public ImmutableArray<TableColumnInstanceSymbol> DefinedValues
        {
            get { return _definedValues; }
        }

        public BoundTableRelation Update(TableInstanceSymbol tableInstance, ImmutableArray<TableColumnInstanceSymbol> definedValues)
        {
            if (tableInstance == _tableInstance && definedValues == _definedValues)
                return this;

            return new BoundTableRelation(tableInstance, definedValues);
        }

        public override IEnumerable<ValueSlot> GetDefinedValues()
        {
            return _definedValues.Select(d => d.ValueSlot);
        }

        public override IEnumerable<ValueSlot> GetOutputValues()
        {
            return GetDefinedValues();
        }

        public override string ToString()
        {
            return _tableInstance.Name;
        }
    }
}