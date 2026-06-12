using NQuery.Binding;

namespace NQuery.Symbols
{
    public sealed class QueryColumnInstanceSymbol : ColumnInstanceSymbol
    {
        private readonly ValueSlot _valueSlot;
        private readonly NQuery.Refactor.Binding.IBoundValue _boundValue;

        internal QueryColumnInstanceSymbol(string name, ValueSlot valueSlot)
            : base(name)
        {
            _valueSlot = valueSlot;
        }

        // Refactor pipeline: a query column always exposes an existing value (a table column or a
        // computed value), so it aliases that value's identity rather than introducing one.
        internal QueryColumnInstanceSymbol(string name, NQuery.Refactor.Binding.IBoundValue boundValue)
            : base(name)
        {
            _boundValue = boundValue;
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.QueryColumnInstance; }
        }

        internal override ValueSlot ValueSlot => _valueSlot ?? throw new InvalidOperationException("This symbol was bound by the AlgebraBinding binder; use BoundValue.");

        internal override NQuery.Refactor.Binding.IBoundValue BoundValue => _boundValue ?? throw new InvalidOperationException("This symbol was bound by the legacy Binding binder; use ValueSlot.");

        private protected override Type SlotType => _valueSlot is not null ? _valueSlot.Type : _boundValue.Type;
    }
}
