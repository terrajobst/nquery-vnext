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

        public sealed override Type Type
        {
            get { return ValueSlot.Type; }
        }
    }
}