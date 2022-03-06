using System.Collections.Immutable;
using NQuery.Binding;

namespace NQuery.Symbols
{
    public sealed class QueryColumnInstanceSymbol : ColumnInstanceSymbol
    {
        internal QueryColumnInstanceSymbol(string name, ValueSlot valueSlot)
            : this(name, valueSlot, ImmutableArray<QueryColumnInstanceSymbol>.Empty)
        {
        }

        internal QueryColumnInstanceSymbol(string name, ValueSlot valueSlot, ImmutableArray<QueryColumnInstanceSymbol> joinedColumns)
            : base(name)
        {
            ValueSlot = valueSlot;
            JoinedColumns = joinedColumns;
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.QueryColumnInstance; }
        }

        internal override ValueSlot ValueSlot { get; }

        public ImmutableArray<QueryColumnInstanceSymbol> JoinedColumns { get; }
    }
}