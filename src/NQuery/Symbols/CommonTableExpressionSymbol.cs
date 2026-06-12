using System.Collections.Immutable;

using NQuery.Binding;

namespace NQuery.Symbols
{
    public sealed class CommonTableExpressionSymbol : TableSymbol
    {
        private readonly BoundQuery _anchor;
        private readonly NQuery.AlgebraBinding.BoundQuery _anchorRefactor;
        private readonly ImmutableArray<BoundQuery> _recursiveMembers;
        private readonly ImmutableArray<NQuery.AlgebraBinding.BoundQuery> _recursiveMembersRefactor;

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
            (_anchor, Columns) = anchorBinder(this);
            _recursiveMembers = recursiveBinder(this);
        }

        internal CommonTableExpressionSymbol(
            string name,
            Func<CommonTableExpressionSymbol, (NQuery.AlgebraBinding.BoundQuery Anchor, ImmutableArray<ColumnSymbol> Columns)> anchorBinder
        )
            : this(name, anchorBinder, _ => ImmutableArray<NQuery.AlgebraBinding.BoundQuery>.Empty)
        {
        }

        internal CommonTableExpressionSymbol(
            string name,
            Func<CommonTableExpressionSymbol, (NQuery.AlgebraBinding.BoundQuery Anchor, ImmutableArray<ColumnSymbol> Columns)> anchorBinder,
            Func<CommonTableExpressionSymbol, ImmutableArray<NQuery.AlgebraBinding.BoundQuery>> recursiveBinder
        )
            : base(name)
        {
            (_anchorRefactor, Columns) = anchorBinder(this);
            _recursiveMembersRefactor = recursiveBinder(this);
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

        internal BoundQuery Anchor => _anchor ?? throw new InvalidOperationException("This CTE was bound by the AlgebraBinding binder; use AnchorRefactor.");

        internal NQuery.AlgebraBinding.BoundQuery AnchorRefactor => _anchorRefactor ?? throw new InvalidOperationException("This CTE was bound by the legacy Binding binder; use Anchor.");

        internal ImmutableArray<BoundQuery> RecursiveMembers => _recursiveMembers.IsDefault ? throw new InvalidOperationException("This CTE was bound by the AlgebraBinding binder; use RecursiveMembersRefactor.") : _recursiveMembers;

        internal ImmutableArray<NQuery.AlgebraBinding.BoundQuery> RecursiveMembersRefactor => _recursiveMembersRefactor.IsDefault ? throw new InvalidOperationException("This CTE was bound by the legacy Binding binder; use RecursiveMembers.") : _recursiveMembersRefactor;
    }
}