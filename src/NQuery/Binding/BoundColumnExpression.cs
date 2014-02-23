using System;

using NQuery.Symbols;

namespace NQuery.Binding
{
    internal sealed class BoundColumnExpression : BoundExpression
    {
        private readonly ColumnInstanceSymbol _symbol;

        public BoundColumnExpression(ColumnInstanceSymbol symbol)
        {
            _symbol = symbol;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.ColumnExpression;  }
        }

        public override Type Type
        {
            get { return _symbol.Type; }
        }

        public ColumnInstanceSymbol Symbol
        {
            get { return _symbol; }
        }

        public override string ToString()
        {
            return _symbol.ToString();
        }
    }
}