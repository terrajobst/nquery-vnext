#if !BASELINE

using System.Collections.Immutable;

using NQuery.Refactor.Emit;
using NQuery.Refactor.Iterators;
using NQuery.Symbols;

namespace NQuery
{
    // New engine: a top-level query is an emitted ExecutablePlan. An instance can instead
    // be backed by the legacy _query (base ctor) when Compilation falls back to the legacy
    // engine for a bare expression -- in that case it is only used via CreateExpressionEvaluator,
    // never CreateReader, so BuildIterator is unreachable for it.
    public sealed partial class CompiledQuery
    {
        private readonly ExecutablePlan _plan;

        internal CompiledQuery(ExecutablePlan plan)
        {
            _plan = plan;
        }

        private ImmutableArray<QueryColumnInstanceSymbol> OutputColumns =>
            _plan is not null ? _plan.OutputColumns : _query.OutputColumns;

        private Iterator BuildIterator()
        {
            if (_plan is null)
                throw new InvalidOperationException("This compiled query is expression-backed; use CreateExpressionEvaluator.");

            return _plan.CreateIterator();
        }
    }
}

#endif
