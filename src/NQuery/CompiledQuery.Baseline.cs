#if BASELINE

using System.Collections.Immutable;

using NQuery.Iterators;
using NQuery.Symbols;

namespace NQuery
{
    // Baseline engine: iterators come from the legacy IteratorBuilder over the BoundQuery.
    public sealed partial class CompiledQuery
    {
        private ImmutableArray<QueryColumnInstanceSymbol> OutputColumns => _query.OutputColumns;

        private Iterator BuildIterator()
        {
            return IteratorBuilder.Build(_query.Relation);
        }
    }
}

#endif
