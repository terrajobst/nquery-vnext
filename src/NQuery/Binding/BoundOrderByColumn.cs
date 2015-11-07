using System;

using NQuery.Symbols;

namespace NQuery.Binding
{
    internal sealed class BoundOrderByColumn : BoundNode
    {
        public BoundOrderByColumn(QueryColumnInstanceSymbol queryColumn, BoundSortedValue sortedValue)
        {
            QueryColumn = queryColumn;
            SortedValue = sortedValue;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.OrderByColumn; }
        }

        public QueryColumnInstanceSymbol QueryColumn { get; }

        public BoundSortedValue SortedValue { get; }
    }
}