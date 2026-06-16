using System.Collections.Immutable;

namespace NQuery.CodeAnalysis.Binding;

internal sealed class BoundGroupByClause
{
    public BoundGroupByClause(IEnumerable<BoundComparedValue> groups)
        : this(groups, ImmutableArray<BoundComputedValue>.Empty)
    {
    }

    public BoundGroupByClause(IEnumerable<BoundComparedValue> groups, ImmutableArray<BoundComputedValue> computedGroups)
    {
        Groups = groups.ToImmutableArray();
        ComputedGroups = computedGroups;
    }

    public ImmutableArray<BoundComparedValue> Groups { get; }

    // GROUP BY expressions that aren't plain column references and therefore must be
    // materialized as value slots before grouping.
    public ImmutableArray<BoundComputedValue> ComputedGroups { get; }
}
