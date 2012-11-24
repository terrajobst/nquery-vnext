using System;

using NQuery.Symbols;

namespace NQuery.Algebra
{
    internal sealed class AlgebraColumnExpression : AlgebraExpression
    {
        private readonly ColumnInstanceSymbol _symbol;

        public AlgebraColumnExpression(ColumnInstanceSymbol symbol)
        {
            _symbol = symbol;
        }

        public override AlgebraKind Kind
        {
            get { return AlgebraKind.ColumnExpression; }
        }

        public ColumnInstanceSymbol Symbol
        {
            get { return _symbol; }
        }
    }
}