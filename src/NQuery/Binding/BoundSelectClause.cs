using System.Collections.Immutable;

namespace NQuery.Binding
{
    internal sealed class BoundSelectClause
    {
        public BoundSelectClause(
            ImmutableArray<BoundSelectColumn> columns,
            bool isDistinct,
            ImmutableArray<BoundAggregatedValue> aggregates,
            ImmutableArray<BoundComputedValue> projections,
            ImmutableArray<BoundComparedValue> distinctValues)
        {
            Columns = columns;
            IsDistinct = isDistinct;
            Aggregates = aggregates;
            Projections = projections;
            DistinctValues = distinctValues;
        }

        public ImmutableArray<BoundSelectColumn> Columns { get; }

        public bool IsDistinct { get; }

        // Aggregates discovered while binding the SELECT/HAVING expressions.
        public ImmutableArray<BoundAggregatedValue> Aggregates { get; }

        // Computed SELECT-list expressions that must be materialized as value slots.
        public ImmutableArray<BoundComputedValue> Projections { get; }

        // The grouping keys used to implement SELECT DISTINCT when there is no ORDER BY (an
        // ORDER BY turns DISTINCT into a distinct-sort over the ORDER BY's SortedValues instead).
        // Empty otherwise.
        public ImmutableArray<BoundComparedValue> DistinctValues { get; }
    }
}
