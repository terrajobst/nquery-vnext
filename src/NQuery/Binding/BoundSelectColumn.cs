using System;

using NQuery.Symbols;

namespace NQuery.Binding
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