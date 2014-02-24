using System;

using NQuery.Symbols;

namespace NQuery.Binding
{
    internal sealed class BoundDerivedTableRelation : BoundRelation
    {
        private readonly TableInstanceSymbol _tableInstance;
        private readonly BoundQueryRelation _query;

        public BoundDerivedTableRelation(TableInstanceSymbol tableInstance, BoundQueryRelation query)
        {
            _tableInstance = tableInstance;
            _query = query;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.DerivedTableRelation; }
        }

        public TableInstanceSymbol TableInstance
        {
            get { return _tableInstance; }
        }

        public BoundQueryRelation Query
        {
            get { return _query; }
        }
    }
}