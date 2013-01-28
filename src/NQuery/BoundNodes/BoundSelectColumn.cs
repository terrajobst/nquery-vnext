using System;

using NQuery.Symbols;

namespace NQuery.BoundNodes
{
    internal sealed class BoundSelectColumn : BoundNode
    {
        private readonly QueryColumnInstanceSymbol _column;

        public BoundSelectColumn(QueryColumnInstanceSymbol column)
        {
            _column = column;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.SelectColumn; }
        }

        public QueryColumnInstanceSymbol Column
        {
            get { return _column; }
        }
    }
}