using System;

using NQuery.Symbols;

namespace NQuery.Algebra
{
    internal sealed class AlgebraTableNode : AlgebraRelation
    {
        private readonly TableInstanceSymbol _symbol;

        public AlgebraTableNode(TableInstanceSymbol symbol)
        {
            _symbol = symbol;
        }

        public override AlgebraKind Kind
        {
            get { return AlgebraKind.Table; }
        }

        public TableInstanceSymbol Symbol
        {
            get { return _symbol; }
        }
    }
}