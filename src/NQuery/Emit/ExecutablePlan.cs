#nullable enable

using System.Collections.Immutable;

using NQuery.EmittedIterators;
using NQuery.Symbols;

namespace NQuery.Emit
{
    // The emitted, reusable artifact: a tree of ExecutableOperators plus the named
    // output columns. CreateIterator() produces a fresh stateful iterator for one
    // execution -- the IEnumerable/IEnumerator relationship.
    //
    // Expression-bearing operators (filter, compute) compile their delegates once at
    // emit time via EmittedExpressionCompiler -- those delegates take the row buffer
    // as a parameter, so they are reused across executions. CreateIterator() then
    // only instantiates fresh stateful iterators (plus the cheap per-run row-buffer
    // entries used by sort/top/project).
    internal sealed class ExecutablePlan
    {
        private readonly ExecutableOperator _root;

        public ExecutablePlan(ExecutableOperator root, ImmutableArray<QueryColumnInstanceSymbol> outputColumns)
        {
            _root = root;
            OutputColumns = outputColumns;
        }

        public ImmutableArray<QueryColumnInstanceSymbol> OutputColumns { get; }

        public Iterator CreateIterator()
        {
            return _root.CreateIterator(outer: null);
        }
    }
}
