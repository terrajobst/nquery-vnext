using System.Collections.Immutable;

using NQuery.Symbols;

namespace NQuery.Binding
{
    // An ORDER BY applied to a set-combined query (UNION/INTERSECT/EXCEPT). For ORDER BY applied
    // to a plain SELECT, the ordering lives on BoundSelectQuery instead -- the binder handles that
    // case while binding the SELECT, because it has to distinguish computed columns from output
    // columns.
    internal sealed class BoundOrderedQuery : BoundQuery
    {
        public BoundOrderedQuery(BoundQuery query, ImmutableArray<BoundComparedValue> sortedValues, ImmutableArray<QueryColumnInstanceSymbol> outputColumns)
        {
            Query = query;
            SortedValues = sortedValues;
            OutputColumns = outputColumns;
        }

        public override BoundNodeKind Kind => BoundNodeKind.OrderedQuery;

        public BoundQuery Query { get; }

        public ImmutableArray<BoundComparedValue> SortedValues { get; }

        public override ImmutableArray<QueryColumnInstanceSymbol> OutputColumns { get; }
    }
}
