using NQuery.Binding;

namespace NQuery.Symbols
{
    public abstract class ColumnInstanceSymbol : Symbol
    {
        private protected ColumnInstanceSymbol(string name)
            : base(name)
        {
        }

        // Legacy (NQuery.Binding) pipeline: the value slot this column resolves to.
        internal abstract ValueSlot ValueSlot { get; }

        // Refactor (NQuery.Refactor.Binding) pipeline: the value identity this column resolves to.
        // The algebrizer maps it to a slot; the symbol carries no slot of its own.
        internal abstract NQuery.Refactor.Binding.IBoundValue BoundValue { get; }

        public sealed override Type Type
        {
            get { return SlotType; }
        }

        // Resolve the type from whichever world's value is populated; ValueSlot/BoundValue each
        // throw when their backing field is null, so we cannot blindly read one here.
        private protected abstract Type SlotType { get; }
    }
}
