using NQuery.Binding;

namespace NQuery.Symbols
{
    public abstract class ColumnInstanceSymbol : Symbol
    {
        private protected ColumnInstanceSymbol(string name)
            : base(name)
        {
        }

        internal abstract ValueSlot ValueSlot { get; }

        internal abstract NQuery.AlgebraBinding.ValueSlot ValueSlotRefactor { get; }

        public sealed override Type Type
        {
            get { return SlotType; }
        }

        // Resolve the type from whichever world's slot is populated; ValueSlot/ValueSlotRefactor
        // each throw when their backing field is null, so we cannot blindly read one here.
        private protected abstract Type SlotType { get; }
    }
}