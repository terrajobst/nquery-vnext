namespace NQuery.Symbols
{
    public abstract class ColumnInstanceSymbol : Symbol
    {
        private protected ColumnInstanceSymbol(string name)
            : base(name)
        {
        }

        // The value identity this column resolves to. The algebrizer maps it to a slot; the
        // symbol carries no slot of its own.
        internal abstract NQuery.Binding.IBoundValue BoundValue { get; }

        public sealed override Type Type
        {
            get { return SlotType; }
        }

        private protected abstract Type SlotType { get; }
    }
}
