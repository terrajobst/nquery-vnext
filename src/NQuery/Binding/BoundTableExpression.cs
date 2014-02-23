using System;

using NQuery.Symbols;

namespace NQuery.Binding
{
    internal sealed class BoundTableExpression : BoundExpression
    {
        private readonly TableInstanceSymbol _symbol;

        public BoundTableExpression(TableInstanceSymbol symbol)
        {
            _symbol = symbol;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.TableExpression; }
        }

        public override Type Type
        {
            get { return _symbol.Type; }
        }

        public TableInstanceSymbol Symbol
        {
            get { return _symbol; }
        }

        public override string ToString()
        {
            return _symbol.ToString();
        }
    }
}