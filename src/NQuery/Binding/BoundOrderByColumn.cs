using NQuery.Symbols;

namespace NQuery.Binding
{
    internal sealed class BoundOrderByColumn : BoundNode
    {
        public BoundOrderByColumn(QueryColumnInstanceSymbol queryColumn, BoundComparedValue comparedValue)
        {
            QueryColumn = queryColumn;
            ComparedValue = comparedValue;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.OrderByColumn; }
        }

        public QueryColumnInstanceSymbol QueryColumn { get; }

        public BoundComparedValue ComparedValue { get; }
    }
}