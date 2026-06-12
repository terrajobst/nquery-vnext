using NQuery.Symbols;

using NQuery.Binding;

namespace NQuery.AlgebraBinding
{
    internal sealed class BoundSelectColumn : BoundNode
    {
        public BoundSelectColumn(QueryColumnInstanceSymbol column)
        {
            Column = column;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.SelectColumn; }
        }

        public QueryColumnInstanceSymbol Column { get; }
    }
}