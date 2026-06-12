using System.Collections.Immutable;

using NQuery.Symbols;

namespace NQuery.AlgebraBinding
{
    // A query that produces no rows and no columns; used for error recovery.
    internal sealed class BoundEmptyQuery : BoundQuery
    {
        public override BoundNodeKind Kind => BoundNodeKind.EmptyQuery;

        public override ImmutableArray<QueryColumnInstanceSymbol> OutputColumns => ImmutableArray<QueryColumnInstanceSymbol>.Empty;
    }
}
