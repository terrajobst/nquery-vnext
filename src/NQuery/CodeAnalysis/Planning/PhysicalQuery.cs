using System.Collections.Immutable;

using NQuery.CodeAnalysis.Symbols;

namespace NQuery.CodeAnalysis.Planning;

internal sealed class PhysicalQuery
{
    public PhysicalQuery(PhysicalOperator root, ImmutableArray<QueryColumnInstanceSymbol> outputColumns)
    {
        ThrowIfNull(root);

        Root = root;
        OutputColumns = outputColumns;
    }

    public PhysicalOperator Root { get; }

    public ImmutableArray<QueryColumnInstanceSymbol> OutputColumns { get; }
}
