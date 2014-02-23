using System;

using NQuery.Symbols;

namespace NQuery.Binding
{
    internal sealed class BoundDerivedTableRelation : BoundRelation
    {
        private readonly BoundRelation _query;
        private readonly TableInstanceSymbol _symbol;

        public BoundDerivedTableRelation(BoundRelation query, TableInstanceSymbol symbol)
        {
            _query = query;
            _symbol = symbol;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.DerivedTableRelation; }
        }

        public BoundRelation Query
        {
            get { return _query; }
        }

        public TableInstanceSymbol Symbol
        {
            get { return _symbol; }
        }
    }
}