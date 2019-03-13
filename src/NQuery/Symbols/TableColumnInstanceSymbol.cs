#nullable enable

using System;

using NQuery.Binding;

namespace NQuery.Symbols
{
    public sealed class TableColumnInstanceSymbol : ColumnInstanceSymbol
    {
        internal TableColumnInstanceSymbol(TableInstanceSymbol tableInstance, ColumnSymbol column, Func<TableInstanceSymbol, ColumnSymbol, ValueSlot> valueSlotFactory)
            : base(column.Name)
        {
            TableInstance = tableInstance;
            Column = column;
            ValueSlot = valueSlotFactory(tableInstance, column);
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.TableColumnInstance; }
        }

        internal override ValueSlot ValueSlot { get; }

        public TableInstanceSymbol TableInstance { get; }

        public ColumnSymbol Column { get; }
    }
}