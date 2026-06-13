#if !BASELINE

using System.Collections.Immutable;

using NQuery.Refactor.Emit;
using NQuery.Refactor.Iterators;
using NQuery.Symbols;

namespace NQuery
{
    // New engine: every compilation -- top-level query and bare expression alike -- is an
    // emitted ExecutablePlan. (A bare expression is wrapped in a one-row projection by the
    // algebrizer, so it plans and emits like any other query.) The legacy _query field stays
    // null in this build; the legacy-backed ctor is used only by the BASELINE build.
    public sealed partial class CompiledQuery
    {
        private readonly ExecutablePlan _plan;

        internal CompiledQuery(ExecutablePlan plan)
        {
            _plan = plan;
        }

        private ImmutableArray<QueryColumnInstanceSymbol> OutputColumns => _plan.OutputColumns;

        private Iterator BuildIterator()
        {
            return _plan.CreateIterator();
        }
    }
}

#endif
