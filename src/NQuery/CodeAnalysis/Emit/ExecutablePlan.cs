using System.Collections.Immutable;

using NQuery.CodeAnalysis.Iterators;
using NQuery.CodeAnalysis.Symbols;

namespace NQuery.CodeAnalysis.Emit;

// The emitted, reusable artifact: a tree of ExecutableOperators plus the named
// output columns. CreateIterator() produces a fresh stateful iterator for one
// execution -- the IEnumerable/IEnumerator relationship.
//
// Expression-bearing operators (filter, compute) compile their delegates once at
// emit time via ExpressionCompiler -- those delegates take the row buffer
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

    // The root operator's output slots, in column order -- used to resolve each output
    // column to its row-buffer address for the reader.
    public ImmutableArray<Algebra.ValueSlot> OutputValueSlots => _root.OutputValueSlots;

    public Iterator CreateIterator()
    {
        // A fresh registry per execution: a recursive union and its reference leaves
        // resolve their shared token against it, so concurrent executions of the same
        // plan never share recursion state.
        return _root.CreateIterator(new RecursiveWorkTableRegistry(), outer: null);
    }
}
