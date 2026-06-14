using System.Collections;
using System.Collections.Immutable;

using NQuery.Symbols;

namespace NQuery.Binding;

internal sealed class BoundUnionQuery : BoundQuery
{
    public BoundUnionQuery(
        bool isUnionAll,
        ImmutableArray<BoundQueryInput> inputs,
        ImmutableArray<BoundUnifiedValue> unifiedValues,
        ImmutableArray<IComparer> comparers,
        ImmutableArray<QueryColumnInstanceSymbol> outputColumns)
    {
        IsUnionAll = isUnionAll;
        Inputs = inputs;
        UnifiedValues = unifiedValues;
        Comparers = comparers;
        OutputColumns = outputColumns;
    }

    public override BoundNodeKind Kind => BoundNodeKind.UnionQuery;

    public bool IsUnionAll { get; }

    public ImmutableArray<BoundQueryInput> Inputs { get; }

    public ImmutableArray<BoundUnifiedValue> UnifiedValues { get; }

    // Empty for UNION ALL; one comparer per output column for UNION (used to de-duplicate).
    public ImmutableArray<IComparer> Comparers { get; }

    public override ImmutableArray<QueryColumnInstanceSymbol> OutputColumns { get; }
}
