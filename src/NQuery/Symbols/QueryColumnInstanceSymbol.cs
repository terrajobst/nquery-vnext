namespace NQuery.Symbols
{
    public sealed class QueryColumnInstanceSymbol : ColumnInstanceSymbol
    {
        private readonly NQuery.Binding.IBoundValue _boundValue;

        // A query column always exposes an existing value (a table column or a computed value),
        // so it aliases that value's identity rather than introducing one.
        internal QueryColumnInstanceSymbol(string name, NQuery.Binding.IBoundValue boundValue)
            : base(name)
        {
            _boundValue = boundValue;
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.QueryColumnInstance; }
        }

        internal override NQuery.Binding.IBoundValue BoundValue => _boundValue;

        private protected override Type SlotType => _boundValue.Type;
    }
}
