using System.Collections.Immutable;

namespace NQuery.Symbols
{
    public sealed class CommonTableExpressionSymbol : TableSymbol
    {
        private readonly NQuery.Binding.BoundQuery _anchor;
        private readonly ImmutableArray<NQuery.Binding.BoundQuery> _recursiveMembers;

        internal CommonTableExpressionSymbol(
            string name,
            Func<CommonTableExpressionSymbol, (NQuery.Binding.BoundQuery Anchor, ImmutableArray<ColumnSymbol> Columns)> anchorBinder
        )
            : this(name, anchorBinder, _ => ImmutableArray<NQuery.Binding.BoundQuery>.Empty)
        {
        }

        internal CommonTableExpressionSymbol(
            string name,
            Func<CommonTableExpressionSymbol, (NQuery.Binding.BoundQuery Anchor, ImmutableArray<ColumnSymbol> Columns)> anchorBinder,
            Func<CommonTableExpressionSymbol, ImmutableArray<NQuery.Binding.BoundQuery>> recursiveBinder
        )
            : base(name)
        {
            (_anchor, Columns) = anchorBinder(this);
            _recursiveMembers = recursiveBinder(this);
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

        internal NQuery.Binding.BoundQuery Anchor => _anchor;

        internal ImmutableArray<NQuery.Binding.BoundQuery> RecursiveMembers => _recursiveMembers;
    }
}
