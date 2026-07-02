namespace NQuery.CodeAnalysis.Iterators;

// The one small shared-by-reference handle connecting the recursion driver
// (RecursionIterator) to its reference leaves (RecursiveReferenceIterator) at run
// time -- unavoidable, because the leaves live inside the member subtrees. The
// driver points Frontier at the store holding the previous round's rows before it
// (re-)opens the members; a reference snapshots the store on Open and scans it.
// One instance exists per recursion per execution: driver and leaves resolve
// their shared recursion token against the per-execution
// RecursiveWorkTableRegistry, so executions don't share state.
internal sealed class RecursiveWorkTable
{
    // The current recursion frontier, or null before the first round. Stable for the
    // duration of a round: the driver appends produced rows to the *other* store and
    // only swaps between rounds.
    public SpooledRowStore? Frontier { get; set; }
}
