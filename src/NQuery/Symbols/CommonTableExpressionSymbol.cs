using System.Collections.Immutable;

using NQuery.Binding;

namespace NQuery.Symbols
{
    public sealed class CommonTableExpressionSymbol : TableSymbol
    {
        internal CommonTableExpressionSymbol(
            string name,
            Func<CommonTableExpressionSymbol, (BoundQuery Anchor, ImmutableArray<ColumnSymbol> Columns)> anchorBinder
        )
            : this(name, anchorBinder, _ => ImmutableArray<BoundQuery>.Empty)
        {
        }

        internal CommonTableExpressionSymbol(
            string name,
            Func<CommonTableExpressionSymbol, (BoundQuery Anchor, ImmutableArray<ColumnSymbol> Columns)> anchorBinder,
            Func<CommonTableExpressionSymbol, ImmutableArray<BoundQuery>> recursiveBinder
        )
            : base(name)
        {
            (Anchor, Columns) = anchorBinder(this);
            RecursiveMembers = recursiveBinder(this);
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.CommonTableExpression; }
        }

        public override Type Type
        {
            get { return TypeFacts.Missing; }
        }

        public override ImmutableArray<ColumnSymbol> Columns { get; }

        internal BoundQuery Anchor { get; }

        internal ImmutableArray<BoundQuery> RecursiveMembers { get; }
    }
}