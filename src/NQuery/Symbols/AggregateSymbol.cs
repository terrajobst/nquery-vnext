using System;

using NQuery.Binding;

namespace NQuery.Symbols
{
    public sealed class AggregateSymbol : Symbol
    {
        public AggregateSymbol(string name)
            : base(name)
        {
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.Aggregate; }
        }

        public override Type Type
        {
            get { return KnownTypes.Missing; }
        }
    }
}