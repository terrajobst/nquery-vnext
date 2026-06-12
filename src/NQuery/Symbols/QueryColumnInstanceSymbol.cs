using NQuery.Binding;

namespace NQuery.Symbols
{
    public sealed class QueryColumnInstanceSymbol : ColumnInstanceSymbol
    {
        private readonly ValueSlot _valueSlot;
        private readonly NQuery.AlgebraBinding.ValueSlot _valueSlotRefactor;

        internal QueryColumnInstanceSymbol(string name, ValueSlot valueSlot)
            : base(name)
        {
            _valueSlot = valueSlot;
        }

        internal QueryColumnInstanceSymbol(string name, NQuery.AlgebraBinding.ValueSlot valueSlot)
            : base(name)
        {
            _valueSlotRefactor = valueSlot;
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.QueryColumnInstance; }
        }

        internal override ValueSlot ValueSlot => _valueSlot ?? throw new InvalidOperationException("This symbol was bound by the AlgebraBinding binder; use ValueSlotRefactor.");

        internal override NQuery.AlgebraBinding.ValueSlot ValueSlotRefactor => _valueSlotRefactor ?? throw new InvalidOperationException("This symbol was bound by the legacy Binding binder; use ValueSlot.");

        private protected override Type SlotType => _valueSlot is not null ? _valueSlot.Type : _valueSlotRefactor.Type;
    }
}