using System.Collections.Immutable;

namespace NQuery.Symbols
{
    public sealed class ErrorTableSymbol : TableSymbol
    {
        internal ErrorTableSymbol(string name)
            : base(name)
        {
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.ErrorTable; }
        }

        public override Type Type
        {
            get { return TypeFacts.Unknown; }
        }

        public override ImmutableArray<ColumnSymbol> Columns
        {
            get { return ImmutableArray<ColumnSymbol>.Empty; }
        }
    }
}