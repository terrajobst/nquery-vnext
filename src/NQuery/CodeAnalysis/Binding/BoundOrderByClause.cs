using System.Collections.Immutable;

namespace NQuery.CodeAnalysis.Binding;

internal sealed class BoundOrderByClause
{
    public BoundOrderByClause(IEnumerable<BoundOrderByColumn> columns, ImmutableArray<BoundComparedValue> sortedValues)
    {
        Columns = [.. columns];
        SortedValues = sortedValues;
    }

    public ImmutableArray<BoundOrderByColumn> Columns { get; }

    // The resolved sort keys: the ORDER BY columns' compared values, plus -- for a SELECT
    // DISTINCT that also has an ORDER BY -- the output columns not already named by ORDER BY
    // (so DISTINCT and ORDER BY agree). This is what the sort actually orders by.
    public ImmutableArray<BoundComparedValue> SortedValues { get; }
}
