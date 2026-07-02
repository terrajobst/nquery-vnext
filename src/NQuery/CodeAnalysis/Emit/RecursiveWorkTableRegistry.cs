using NQuery.CodeAnalysis.Algebra;

using NQuery.CodeAnalysis.Iterators;

namespace NQuery.CodeAnalysis.Emit;

// The per-execution registry resolving a recursion's identity to its
// RecursiveWorkTable. An ExecutableRecursiveUnion and its
// ExecutableRecursiveReference leaves sit in different subtrees, so neither can
// hand the other the work table directly; instead both carry the same
// RecursionToken and lazily resolve it here -- whichever node is created first
// creates the table. ExecutablePlan.CreateIterator mints a fresh registry per
// call and threads it through the whole tree build, so concurrent executions of
// one plan never share recursion state.
internal sealed class RecursiveWorkTableRegistry
{
    private readonly Dictionary<RecursionToken, RecursiveWorkTable> _workTables = new();

    public RecursiveWorkTable GetOrCreate(RecursionToken token)
    {
        if (!_workTables.TryGetValue(token, out var workTable))
        {
            workTable = new RecursiveWorkTable();
            _workTables.Add(token, workTable);
        }

        return workTable;
    }
}
