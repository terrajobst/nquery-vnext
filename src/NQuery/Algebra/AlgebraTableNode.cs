using System;

using NQuery.Symbols;

namespace NQuery.Algebra
{
    internal sealed class AlgebraTableNode : AlgebraNode
    {
        private readonly TableInstanceSymbol _symbol;

        public AlgebraTableNode(TableInstanceSymbol symbol)
        {
            _symbol = symbol;
        }

        public TableInstanceSymbol Symbol
        {
            get { return _symbol; }
        }

        public override AlgebraKind Kind
        {
            get { return AlgebraKind.Table; }
        }
    }
}