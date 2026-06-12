using NQuery.Binding;

namespace NQuery.Symbols
{
    public sealed class TableColumnInstanceSymbol : ColumnInstanceSymbol, NQuery.Refactor.Binding.IBoundValue
    {
        private readonly ValueSlot _valueSlot;
        private readonly bool _isRefactor;
        private readonly NQuery.Refactor.Binding.IBoundValue _aliased;

        internal TableColumnInstanceSymbol(TableInstanceSymbol tableInstance, ColumnSymbol column, Func<TableInstanceSymbol, ColumnSymbol, ValueSlot> valueSlotFactory)
            : base(column.Name)
        {
            TableInstance = tableInstance;
            Column = column;
            _valueSlot = valueSlotFactory(tableInstance, column);
        }

        // Refactor pipeline, real table: the column is its own value identity (its slot is minted
        // by the algebrizer at the table scan).
        internal TableColumnInstanceSymbol(TableInstanceSymbol tableInstance, ColumnSymbol column)
            : base(column.Name)
        {
            TableInstance = tableInstance;
            Column = column;
            _isRefactor = true;
        }

        // Refactor pipeline, derived table: the column aliases the inner query's value, so it
        // resolves to that value rather than introducing one of its own.
        internal TableColumnInstanceSymbol(TableInstanceSymbol tableInstance, ColumnSymbol column, NQuery.Refactor.Binding.IBoundValue aliased)
            : base(column.Name)
        {
            TableInstance = tableInstance;
            Column = column;
            _isRefactor = true;
            _aliased = aliased;
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.TableColumnInstance; }
        }

        internal override ValueSlot ValueSlot => _valueSlot ?? throw new InvalidOperationException("This symbol was bound by the AlgebraBinding binder; use BoundValue.");

        internal override NQuery.Refactor.Binding.IBoundValue BoundValue => _isRefactor
            ? _aliased ?? this
            : throw new InvalidOperationException("This symbol was bound by the legacy Binding binder; use ValueSlot.");

        private protected override Type SlotType => _valueSlot is not null ? _valueSlot.Type : Column.Type;

        public TableInstanceSymbol TableInstance { get; }

        public ColumnSymbol Column { get; }
    }
}
