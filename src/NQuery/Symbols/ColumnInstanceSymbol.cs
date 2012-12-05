using System;

using NQuery.Binding;

namespace NQuery.Symbols
{
    public abstract class ColumnInstanceSymbol : Symbol
    {
        internal ColumnInstanceSymbol(string name)
            : base(name)
        {
        }

        internal abstract ValueSlot ValueSlot { get; }

        public override sealed Type Type
        {
            get { return ValueSlot.Type; }
        }
    }
}