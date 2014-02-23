using System;

using NQuery.Symbols;

namespace NQuery.Binding
{
    internal sealed class BoundTableRelation : BoundRelation
    {
        private readonly TableInstanceSymbol _symbol;

        public BoundTableRelation(TableInstanceSymbol symbol)
        {
            _symbol = symbol;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.TableRelation; }
        }

        public TableInstanceSymbol Symbol
        {
            get { return _symbol; }
        }
    }
}