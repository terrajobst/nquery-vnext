#nullable enable

using System.Collections.Immutable;

using NQuery.Symbols;

namespace NQuery.Refactor.Planning
{
    internal sealed class PhysicalQuery
    {
        public PhysicalQuery(PhysicalOperator root, ImmutableArray<QueryColumnInstanceSymbol> outputColumns)
        {
            Root = root;
            OutputColumns = outputColumns;
        }

        public PhysicalOperator Root { get; }

        public ImmutableArray<QueryColumnInstanceSymbol> OutputColumns { get; }
    }
}
