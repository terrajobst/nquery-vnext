using System;

using NQuery.Symbols;

namespace NQuery.Binding
{
    internal sealed class BoundDerivedTableRelation : BoundRelation
    {
        private readonly TableInstanceSymbol _tableInstance;
        private readonly BoundRelation _relation;

        public BoundDerivedTableRelation(TableInstanceSymbol tableInstance, BoundRelation relation)
        {
            _tableInstance = tableInstance;
            _relation = relation;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.DerivedTableRelation; }
        }

        public TableInstanceSymbol TableInstance
        {
            get { return _tableInstance; }
        }

        public BoundRelation Relation
        {
            get { return _relation; }
        }
    }
}