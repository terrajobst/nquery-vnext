namespace NQuery.Symbols
{
    public sealed class TableColumnInstanceSymbol : ColumnInstanceSymbol, NQuery.Binding.IBoundValue
    {
        private readonly NQuery.Binding.IBoundValue _aliased;

        // Real table: the column is its own value identity (its slot is minted by the algebrizer
        // at the table scan).
        internal TableColumnInstanceSymbol(TableInstanceSymbol tableInstance, ColumnSymbol column)
            : base(column.Name)
        {
            TableInstance = tableInstance;
            Column = column;
        }

        // Derived table: the column aliases the inner query's value, so it resolves to that value
        // rather than introducing one of its own.
        internal TableColumnInstanceSymbol(TableInstanceSymbol tableInstance, ColumnSymbol column, NQuery.Binding.IBoundValue aliased)
            : base(column.Name)
        {
            TableInstance = tableInstance;
            Column = column;
            _aliased = aliased;
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.TableColumnInstance; }
        }

        internal override NQuery.Binding.IBoundValue BoundValue => _aliased ?? this;

        private protected override Type SlotType => Column.Type;

        public TableInstanceSymbol TableInstance { get; }

        public ColumnSymbol Column { get; }
    }
}
