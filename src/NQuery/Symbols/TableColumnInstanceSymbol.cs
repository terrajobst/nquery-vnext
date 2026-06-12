using NQuery.Binding;

namespace NQuery.Symbols
{
    public sealed class TableColumnInstanceSymbol : ColumnInstanceSymbol
    {
        private readonly ValueSlot _valueSlot;
        private readonly NQuery.Refactor.Binding.ValueSlot _valueSlotRefactor;

        internal TableColumnInstanceSymbol(TableInstanceSymbol tableInstance, ColumnSymbol column, Func<TableInstanceSymbol, ColumnSymbol, ValueSlot> valueSlotFactory)
            : base(column.Name)
        {
            TableInstance = tableInstance;
            Column = column;
            _valueSlot = valueSlotFactory(tableInstance, column);
        }

        internal TableColumnInstanceSymbol(TableInstanceSymbol tableInstance, ColumnSymbol column, Func<TableInstanceSymbol, ColumnSymbol, NQuery.Refactor.Binding.ValueSlot> valueSlotFactory)
            : base(column.Name)
        {
            TableInstance = tableInstance;
            Column = column;
            _valueSlotRefactor = valueSlotFactory(tableInstance, column);
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.TableColumnInstance; }
        }

        internal override ValueSlot ValueSlot => _valueSlot ?? throw new InvalidOperationException("This symbol was bound by the AlgebraBinding binder; use ValueSlotRefactor.");

        internal override NQuery.Refactor.Binding.ValueSlot ValueSlotRefactor => _valueSlotRefactor ?? throw new InvalidOperationException("This symbol was bound by the legacy Binding binder; use ValueSlot.");

        private protected override Type SlotType => _valueSlot is not null ? _valueSlot.Type : _valueSlotRefactor.Type;

        public TableInstanceSymbol TableInstance { get; }

        public ColumnSymbol Column { get; }
    }
}