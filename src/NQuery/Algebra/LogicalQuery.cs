using System.Collections.Immutable;

using NQuery.Symbols;

namespace NQuery.Algebra;

// The logical counterpart of BoundQuery: the root operator plus the named
// output columns. Output columns are reused from the binding layer for now.
internal sealed class LogicalQuery
{
    public LogicalQuery(LogicalOperator root, ImmutableArray<QueryColumnInstanceSymbol> outputColumns)
    {
        Root = root;
        OutputColumns = outputColumns;
    }

    public LogicalOperator Root { get; }

    public ImmutableArray<QueryColumnInstanceSymbol> OutputColumns { get; }
}
