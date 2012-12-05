using System;

using NQuery.Binding;

namespace NQuery.Symbols
{
    public sealed class TableColumnInstanceSymbol : ColumnInstanceSymbol
    {
        private readonly TableInstanceSymbol _tableInstance;
        private readonly ColumnSymbol _column;
        private readonly ValueSlot _valueSlot;

        internal TableColumnInstanceSymbol(TableInstanceSymbol tableInstance, ColumnSymbol column, Func<TableInstanceSymbol, ColumnSymbol, ValueSlot> valueSlotFactory)
            : base(column.Name)
        {
            _tableInstance = tableInstance;
            _column = column;
            _valueSlot = valueSlotFactory(tableInstance, column);
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.TableColumnInstance; }
        }

        internal override ValueSlot ValueSlot
        {
            get { return _valueSlot; }
        }

        public TableInstanceSymbol TableInstance
        {
            get { return _tableInstance; }
        }

        public ColumnSymbol Column
        {
            get { return _column; }
        }
    }
}